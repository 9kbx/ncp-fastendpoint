using My.FastNCP.Domain.DomainEvents;
using My.FastNCP.Web.Application.Commands;
using MediatR;
using NetCorePal.Extensions.Domain;

namespace My.FastNCP.Web.Application.DomainEventHandlers
{
    internal class OrderCreatedDomainEventHandler(IMediator mediator) : IDomainEventHandler<OrderCreatedDomainEvent>
    {
        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            return mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
        }
    }
}