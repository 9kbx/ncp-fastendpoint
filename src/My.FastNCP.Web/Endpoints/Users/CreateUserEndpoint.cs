using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

public record CreateUserRequest(string Username, string Password);

public class CreateUserEndpoint : Endpoint<CreateUserRequest, ResponseData<long>>
{
    public override void Configure()
    {
        Post("/api/user/create");
        Permissions("user.create");
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var createdId = Random.Shared.NextInt64(10000000, 99999999);
        await SendOkAsync(createdId.AsResponseData(), ct);
    }
}