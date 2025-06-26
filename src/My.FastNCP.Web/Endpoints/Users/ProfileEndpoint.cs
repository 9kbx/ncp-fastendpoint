using FastEndpoints;
using My.FastNCP.Web.AspNetCore;

namespace My.FastNCP.Web.Endpoints.Users;

public class ProfileEndpoint(ICurrentUser currentUser) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/user/profile");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(currentUser, ct);
    }
}