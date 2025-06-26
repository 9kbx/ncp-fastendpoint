using NetCorePal.Extensions.Domain;
using My.FastNCP.Domain.AggregatesModel.OrderAggregate;

namespace My.FastNCP.Domain.DomainEvents
{
    public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;
}
