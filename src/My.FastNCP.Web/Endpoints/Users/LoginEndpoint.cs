using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;

namespace My.FastNCP.Web.Endpoints.Users;

public record LoginRequest(string Username, string Password);

public class LoginEndpoint : Endpoint<LoginRequest, TokenResponse>
{
    public override void Configure()
    {
        Post("/api/user/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = new
        {
            UserId = 123,
            Username = req.Username,
        };

        Response = await CreateTokenWith<MyTokenService>(user.UserId.ToString(), u =>
        {
            // u.Roles.AddRange(new[] { "Admin", "Manager" });
            // u.Permissions.AddRange("pms1", "pms2", "user.create");
            u.Claims.AddRange(
            [
                new Claim("ClientID", "Default"),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, req.Username),
                new Claim(ClaimTypes.Role, "admin"),
            ]);
        });
    }
}

// accessToken管理
// https://fast-endpoints.com/docs/security#jwt-refresh-tokens
public class MyTokenService : RefreshTokenService<TokenRequest, TokenResponse>
{
    public MyTokenService(IConfiguration config)
    {
        Setup(o =>
        {
            o.TokenSigningKey = config["Auth:Jwt:TokenSigningKey"];
            o.AccessTokenValidity = TimeSpan.FromMinutes(5);
            o.RefreshTokenValidity = TimeSpan.FromHours(4);
            o.Issuer = "my-first-ncp";
            o.Audience = "my-first-ncp";

            // token刷新路由（提交 TokenRequest 对象到此路由）
            o.Endpoint("/api/user/refresh-token",
                ep => { ep.Summary(s => s.Summary = "this is the refresh token endpoint"); });
        });
    }

    // 保存当前用户刷新token
    public override async Task PersistTokenAsync(TokenResponse rsp)
    {
        // Data.StoreToken(rsp.UserId, rsp.RefreshExpiry, rsp.RefreshToken);

        // this method will be called whenever a new access/refresh token pair is being generated.
        // store the tokens and expiry dates however you wish for the purpose of verifying
        // future refresh requests.
        // 每当生成新的访问/刷新令牌对时，都会调用此方法。
        // 以您希望的方式存储令牌和到期日期，以便验证未来的刷新请求。
    }

    // 验证refreshToken是否有效
    public override async Task RefreshRequestValidationAsync(TokenRequest req)
    {
        // if (!await Data.TokenIsValid(req.UserId, req.RefreshToken))
        //     AddError(r => r.RefreshToken, "Refresh token is invalid!");

        // validate the incoming refresh request by checking the token and expiry against the
        // previously stored data. if the token is not valid and a new token pair should
        // not be created, simply add validation errors using the AddError() method.
        // the failures you add will be sent to the requesting client. if no failures are added,
        // validation passes and a new token pair will be created and sent to the client.        
        // 通过检查令牌及其有效期与先前存储的数据是否一致来验证传入的刷新请求。
        // 如果令牌无效且不应创建新的令牌对，则只需使用 AddError() 方法添加验证错误即可。
        // 您添加的失败信息将发送给请求客户端。如果没有添加任何失败信息，
        // 则验证通过，并将创建新的令牌对并发送给客户端。
    }

    // refreshToken验证有效后重新生成accessToken
    public override Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
    {
        // privileges.Roles.Add("Manager");
        // privileges.Claims.Add(new("ManagerID", request.UserId));
        // privileges.Permissions.Add("Manage_Department");

        // var user = load user by cache (request.UserId)
        var user = new
        {
            UserId = request.UserId,
            Username = "admin",
        };

        // privileges.Roles.AddRange(new[] { "Admin", "Manager" });
        // privileges.Permissions.AddRange("pms1", "pms2", "user.create");
        privileges.Claims.AddRange(
        [
            new Claim("ClientID", "Default"),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "admin"),
        ]);

        // specify the user privileges to be embedded in the jwt when a refresh request is
        // received and validation has passed. this only applies to renewal/refresh requests
        // received to the refresh endpoint and not the initial jwt creation.        
        // 指定在收到刷新请求并通过验证后嵌入到 JWT 中的用户权限。这仅适用于刷新端点接收到的续订/刷新请求，而不适用于初始 JWT 创建。

        return Task.CompletedTask;
    }
}

// JWT 令牌注销
// 使用提供的抽象中间件类可以轻松实现令牌吊销。重写 JwtTokenIsValidAsync() 方法，
// 并在检查数据库或吊销令牌缓存后，如果提供的令牌不再有效，则返回 false。
public class MyBlacklistChecker(RequestDelegate next) : JwtRevocationMiddleware(next)
{
    protected override Task<bool> JwtTokenIsValidAsync(string jwtToken, CancellationToken ct)
    {
        // return true if the supplied token is still valid
        return Task.FromResult(true);
    }
}