using FastEndpoints;
using My.FastNCP.Web.Application.Commands;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

// 定义webapi接口
sealed class CreateUserEndpoint(IMediator mediator) : Endpoint<CreateUserRequest, ResponseData<CreateUserResponse>>
{
    // api配置
    public override void Configure()
    {
        Post("/api/user/create"); // api路由
        Description(x => x.WithTags("User")); // 路由分组
        // AllowAnonymous(); // 匿名访问，不调用则需要身份认证后才能访问
        Permissions("user.create"); // 权限验证，拥有user.create权限的用户才能访问
    }

    // 业务逻辑代码
    public override async Task HandleAsync(CreateUserRequest r, CancellationToken c)
    {
        // var password = PasswordHelper.NewPassword();
        var password = "1231231";
        var cmd = new CreateUserCommand(r.UserName, password);
        var result = await mediator.Send(cmd, c);
        var res = new CreateUserResponse(result.UserId, result.UserName);
        await SendOkAsync(res.AsResponseData(), c);
        
        // var payload = new UserInfoRequest(result.UserId);
        // await SendCreatedAtAsync<UserInfoEndpoint>(payload, cancellation: c);
    }
}

/// <summary>
/// 创建用户请求Payload
/// </summary>
/// <param name="UserName">用户名</param>
sealed record CreateUserRequest(string UserName);

/// <summary>
/// 创建用户的响应数据
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="UserName">用户名</param>
sealed record CreateUserResponse(long UserId, string UserName);

// webapi请求对象验证器
sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
    }
}

// webapi描述内容
sealed class CreateUserSummary : Summary<CreateUserEndpoint, CreateUserRequest>
{
    public CreateUserSummary()
    {
        Summary = "创建用户";
        Description = "用户密码由系统随机生成";
        // 给webapi文档添加请求响应示例
        RequestExamples.Add(new RequestExample(new CreateUserRequest("admin"), "创建用户示例1"));
        RequestExamples.Add(new RequestExample(new CreateUserRequest("admin22"), "创建用户示例2"));
        ResponseExamples.Add(200, new CreateUserResponse(123,"admin").AsResponseData());
    }
}