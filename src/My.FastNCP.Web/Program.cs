using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using FastEndpoints.Security;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using FluentValidation.AspNetCore;
using My.FastNCP.Web.Application.Queries;
using My.FastNCP.Web.Application.IntegrationEventHandlers;
using My.FastNCP.Web.Clients;
using Serilog;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Net.Http.Headers;
using My.FastNCP.Web;
using My.FastNCP.Web.AspNetCore;
using My.FastNCP.Web.AspNetCore.ApiKey;
using My.FastNCP.Web.AspNetCore.Middlewares;
using My.FastNCP.Web.AspNetCore.Permission;
using My.FastNCP.Web.Endpoints.Users;
using My.FastNCP.Web.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using Scalar.AspNetCore;
using SystemClock = NetCorePal.Extensions.Primitives.SystemClock;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    // .WriteTo.Console(new JsonFormatter())
    .WriteTo.Console()
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    #region SignalR

    builder.Services.AddHealthChecks();
    builder.Services.AddMvc()
        .AddNewtonsoftJson(options => { options.SerializerSettings.AddNetCorePalJsonConverters(); });
    builder.Services.AddSignalR();

    #endregion

    #region Prometheus监控

    builder.Services.AddHealthChecks().ForwardToPrometheus();
    builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();

    #endregion

    // Add services to the container.

    #region 身份认证

    var redis = await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => redis);
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

    // builder.Services.AddAuthentication().AddJwtBearer(options =>
    // {
    //     options.RequireHttpsMetadata = false;
    //     options.TokenValidationParameters.ValidAudience = "netcorepal";
    //     options.TokenValidationParameters.ValidateAudience = true;
    //     options.TokenValidationParameters.ValidIssuer = "netcorepal";
    //     options.TokenValidationParameters.ValidateIssuer = true;
    // });


    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddTransient<UserPermissionService>(); // 获取用户权限
    builder.Services.AddTransient<IClaimsTransformation, UserPermissionHydrator>(); // 用户权限验证

    builder.Services
        // 添加Jwt身份认证方案
        .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration["Auth:Jwt:TokenSigningKey"])
        .AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = "Jwt_Or_ApiKey";
            o.DefaultChallengeScheme = "Jwt_Or_ApiKey";
        })
        // 添加 ApiKey 身份认证方案
        .AddScheme<AuthenticationSchemeOptions, ApikeyAuth>(ApikeyAuth.SchemeName, null)
        // 综合认证方案（使用jwt或apikey任意一个方案请求endpoint）
        // https://fast-endpoints.com/docs/security#combined-authentication-scheme
        .AddPolicyScheme("Jwt_Or_ApiKey", "Jwt_Or_ApiKey", o =>
        {
            o.ForwardDefaultSelector = ctx =>
            {
                if ((ctx.Request.Headers.TryGetValue(ApikeyAuth.HeaderName, out var apikeyHeader) &&
                     !string.IsNullOrWhiteSpace(apikeyHeader)) ||
                    (ctx.Request.Query.TryGetValue(ApikeyAuth.HeaderName, out apikeyHeader) &&
                     !string.IsNullOrWhiteSpace(apikeyHeader)))
                {
                    return ApikeyAuth.SchemeName;
                }

                if (ctx.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader) &&
                    authHeader.FirstOrDefault()?.StartsWith("Bearer ") is true)
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                return ApikeyAuth.SchemeName;
            };
        });

    builder.Services.AddNetCorePalJwt().AddRedisStore();

    #endregion

    #region Controller

    builder.Services.AddControllers().AddNetCorePalSystemTextJson();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.AddEntityIdSchemaMap()); //强类型id swagger schema 映射

    #endregion

    #region FastEndpoints

    builder.Services
        .AddFastEndpoints(o => { o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All; })
        .SwaggerDocument(o =>
        {
            o.EnableJWTBearerAuth = false; // 禁用SwaggerUI需要登陆才能访问
            o.TagDescriptions = t =>
            {
                t["User"] = "用户接口";
                t["Auth"] = "身份认证接口";
                t["Notification"] = "消息通知接口";
            };
        });
    builder.Services.Configure<JsonOptions>(o =>
        o.SerializerOptions.AddNetCorePalJsonConverters());

    #endregion

    #region 公共服务

    builder.Services.AddSingleton<IClock, SystemClock>();

    #endregion

    #region 集成事件

    builder.Services.AddTransient<OrderPaidIntegrationEventHandler>();

    #endregion

    #region 模型验证器

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    builder.Services.AddKnownExceptionErrorModelInterceptor();

    #endregion

    #region Query

    builder.Services.AddScoped<OrderQuery>();
    builder.Services.AddScoped<UserQuery>();

    #endregion


    #region 基础设施

    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
            new MySqlServerVersion(new Version(8, 0, 34)));
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    builder.Services.AddRedisLocks();
    builder.Services.AddContext().AddEnvContext().AddCapContextProcessor();
    builder.Services.AddNetCorePalServiceDiscoveryClient();
    builder.Services.AddIntegrationEvents(typeof(Program))
        .UseCap<ApplicationDbContext>(b =>
        {
            b.RegisterServicesFromAssemblies(typeof(Program));
            b.AddContextIntegrationFilters();
            b.UseMySql();
        });


    builder.Services.AddCap(x =>
    {
        x.JsonSerializerOptions.AddNetCorePalJsonConverters();
        x.UseEntityFramework<ApplicationDbContext>();
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
        x.UseDashboard(); //CAP Dashboard  path：  /cap
    });

    #endregion

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()
            .AddKnownExceptionValidationBehavior()
            .AddUnitOfWorkBehaviors());

    #region 多环境支持与服务注册发现

    builder.Services.AddMultiEnv(envOption => envOption.ServiceName = "Abc.Template")
        .UseMicrosoftServiceDiscovery();
    builder.Services.AddConfigurationServiceEndpointProvider();
    builder.Services.AddEnvFixedConnectionChannelPool();

    #endregion

    #region 远程服务客户端配置

    var jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
    jsonSerializerSettings.AddNetCorePalJsonConverters();
    var ser = new NewtonsoftJsonContentSerializer(jsonSerializerSettings);
    var settings = new RefitSettings(ser);
    builder.Services.AddRefitClient<IUserServiceClient>(settings)
        .ConfigureHttpClient(client =>
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("https+http://user:8080")!))
        .AddMultiEnvMicrosoftServiceDiscovery() //多环境服务发现支持
        .AddStandardResilienceHandler(); //添加标准的重试策略

    #endregion

    #region Jobs

    builder.Services.AddHangfire(x => { x.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")); });
    builder.Services.AddHangfireServer(); //hangfire dashboard  path：  /hangfire

    #endregion


    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    app.UseKnownExceptionHandler();

    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseJwtRevocation<MyBlacklistChecker>(); // 自定义jwtToken有效性检查中间件
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<CurrentUserMiddleware>(); // 在当前请求范围补充ICurrentUser实例


    app.MapControllers();
    app.UseFastEndpoints(c =>
    {
        // 权限代码默认存储在ClaimType为permission的声明中
        // 假设要自定义可修改这里
        // c.Security.PermissionsClaimType = "从指定ClaimType验证权限";
        // 其他的依此类推
        // c.Security.RoleClaimType = ClaimTypes.Role; 

        c.Binding.ReflectionCache.AddFromMyFastNCPWeb();
    });


    #region SignalR

    app.MapHub<My.FastNCP.Web.Application.Hubs.ChatHub>("/chat");

    #endregion

    app.UseHttpMetrics();
    app.MapHealthChecks("/health");
    app.MapMetrics("/metrics"); // 通过   /metrics  访问指标
    app.UseHangfireDashboard();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // app.UseSwagger();
        // app.UseSwaggerUI();
        app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
        app.MapScalarApiReference();
        app.UseSwaggerGen();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

#pragma warning disable S1118
public partial class Program
#pragma warning restore S1118
{
}