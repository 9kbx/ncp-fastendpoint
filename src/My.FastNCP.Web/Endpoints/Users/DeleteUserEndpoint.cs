using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;

public class DeleteUserRequest
{
    [RouteParam] public long UserId { get; set; }
}

public class DeleteUserEndpoint : Endpoint<DeleteUserRequest, ResponseData>
{
    public override void Configure()
    {
        Delete("/api/user/{userId}/delete");
        // Roles("Admin");
        Permissions("user.delete");
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        await SendOkAsync(true.AsResponseData(), ct);
    }
}