using My.FastNCP.Domain.AggregatesModel.OrderAggregate;

namespace My.FastNCP.Web.Application.IntegrationEventHandlers
{
    public record OrderPaidIntegrationEvent(OrderId OrderId);
}
