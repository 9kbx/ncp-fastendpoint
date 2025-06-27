using FastEndpoints;
using My.FastNCP.Web.Application.Commands;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

public record CreateUserRequest(string Username, string Password);

public class CreateUserEndpoint(IMediator mediator)
    : Endpoint<CreateUserRequest, ResponseData<CreateUserCommandResponse>>
{
    public override void Configure()
    {
        Post("/api/user/create");
        Permissions("user.create");
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var cmd = new CreateUserCommand(req.Username, req.Password);
        var res = await mediator.Send(cmd, ct);
        await SendOkAsync(res.AsResponseData(), ct);
    }
}