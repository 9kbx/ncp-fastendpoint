using My.FastNCP.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Domain;

namespace My.FastNCP.Domain.DomainEvents;

public record OrderPaidDomainEvent(Order Order) : IDomainEvent;