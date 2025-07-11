using FastEndpoints;
using My.FastNCP.Web.AspNetCore;

namespace My.FastNCP.Web.Endpoints.Users;

public class ProfileEndpoint(ICurrentUser currentUser) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/user/profile");
        Description(x => x.WithTags("User"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(currentUser, ct);
    }
}

sealed class ProfileSummary : Summary<ProfileEndpoint>
{
    public ProfileSummary()
    {
        Summary = "用户信息";
        Description = "Description text goes here...";
        
    }
}