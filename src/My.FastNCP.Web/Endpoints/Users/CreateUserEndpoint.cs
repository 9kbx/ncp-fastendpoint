using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

public record CreateUserRequest(string Username, string Password);

public class CreateUserEndpoint : Endpoint<CreateUserRequest, ResponseData>
{
    public override void Configure()
    {
        Post("/api/user/create");
        Permissions("user.create");
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        await SendOkAsync(true.AsResponseData(), ct);
    }
}