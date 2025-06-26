using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using My.FastNCP.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Repository;

namespace My.FastNCP.Infrastructure.Repositories
{

    public interface IOrderRepository : IRepository<Order, OrderId>
    {

    }


    public class OrderRepository(ApplicationDbContext context) : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
    {
    }
}
