using FastEndpoints;
using My.FastNCP.Web.Application.Commands;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

sealed class CreateUserEndpoint(IMediator mediator) : Endpoint<CreateUserRequest, ResponseData<CreateUserResponse>>
{
    public override void Configure()
    {
        Post("/api/user/create");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserRequest r, CancellationToken c)
    {
        var cmd = new CreateUserCommand(r.UserName, r.Password);
        var result = await mediator.Send(cmd, c);
        var res = new CreateUserResponse(result.UserId, result.UserName);
        await SendOkAsync(res.AsResponseData(), c);
        
    }
}

sealed record CreateUserRequest(string UserName, string Password);

sealed record CreateUserResponse(long UserId, string UserName);

sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        // RuleFor(x => x.Property).NotEmpty();
    }
}

sealed class CreateUserSummary : Summary<CreateUserEndpoint, CreateUserRequest>
{
    public CreateUserSummary()
    {
        Summary = "创建用户";
        Description = "Description text goes here...";
    }
}