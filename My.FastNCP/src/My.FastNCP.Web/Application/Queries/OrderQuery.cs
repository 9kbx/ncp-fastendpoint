using My.FastNCP.Domain;
using My.FastNCP.Domain.AggregatesModel.OrderAggregate;
using My.FastNCP.Infrastructure;
using System.Threading;

namespace My.FastNCP.Web.Application.Queries
{
    public class OrderQuery(ApplicationDbContext applicationDbContext)
    {
        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
