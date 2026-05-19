using BuildingBlocks.Messaging.Events;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Domain.Enums;

namespace Ordering.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler
    (ISender sender, ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        //TODO: Create new order and start order fullfillment process
        logger.LogInformation("Integration event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var command = MapToCreateOrderCommand(context.Message);
        await sender.Send(command);
    }

    private static CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
    {
        var addressDto = new AddressDto(message.FirstName, message.LastName, message.EmailAddress, message.AddressLine, message.Country, message.State, message.ZipCode);

        var paymentDto = new PaymentDto(message.CardName, message.CardNumber, message.Expiration, message.CVV, message.PaymentMethod);

        var orderId = Guid.NewGuid();

        var orderDto = new OrderDto
        (
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: $"ORD_{orderId}",
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Status: OrderStatus.Pending,
            OrderItems: message.OrderItems.Select(item => new OrderItemDto(orderId, item.ProductId, item.Quantity, item.Price)).ToList()
        );

        return new CreateOrderCommand(orderDto);
    }
}
