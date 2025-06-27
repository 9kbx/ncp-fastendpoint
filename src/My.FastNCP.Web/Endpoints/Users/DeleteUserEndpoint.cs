using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace My.FastNCP.Web.Endpoints.Users;


sealed class DeleteUserEndpoint : Endpoint<DeleteUserRequest, ResponseData>
{
    public override void Configure()
    {
        Delete("/api/user/{userId}/delete");
        Permissions("user.delete");
    }

    public override async Task HandleAsync(DeleteUserRequest r, CancellationToken c)
    {
        await SendOkAsync(true.AsResponseData(), c);
    }
}

sealed class DeleteUserRequest
{

    [RouteParam] public long UserId { get; set; }
}

sealed class DeleteUserValidator : Validator<DeleteUserRequest>
{
    public DeleteUserValidator()
    {

    }
}

sealed class DeleteUserSummary : Summary<DeleteUserEndpoint, DeleteUserRequest>
{
    public DeleteUserSummary()
    {
        Summary = "删除用户";
        Description = "";
    }
}