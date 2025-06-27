namespace My.FastNCP.Web.Application.Queries;

public class UserQuery(ApplicationDbContext applicationDbContext)
{
    public Task<object?> QueryOrder(long userId, CancellationToken cancellationToken)
    {
        var obj = new
        {
            UserId = userId,
            Name = "Admin"
        };
        return Task.FromResult<object>(obj);
    }
}