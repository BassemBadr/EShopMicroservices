using BuildingBlocks.Messaging.Events;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Domain.Enums;

namespace Ordering.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler
    (ISender sender, IApplicationDbContext dbContext, ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        //TODO: Create new order and start order fullfillment process
        logger.LogInformation("Integration event handled: {IntegrationEvent}", context.Message.GetType().Name);

        await EnsureCustomerExistsAsync(context.Message, context.CancellationToken);

        var command = MapToCreateOrderCommand(context.Message);
        await sender.Send(command);
    }

    /// <summary>
    /// The customer id now comes from the caller's token, so the first order of a new user has to
    /// create the row that the required <c>Order.CustomerId</c> foreign key points at.
    /// </summary>
    private async Task EnsureCustomerExistsAsync(BasketCheckoutEvent message, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Of(message.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId, cancellationToken);

        if (customerExists) return;

        // Customer.Email has a unique index and Customer.Create rejects empty values, so fall back
        // to a deterministic address when the checkout form did not provide one.
        var email = string.IsNullOrWhiteSpace(message.EmailAddress)
            ? $"{customerId.Value:N}@users.invalid"
            : message.EmailAddress;

        dbContext.Customers.Add(Customer.Create(customerId, message.UserName, email));

        await dbContext.SaveChangesAsync(cancellationToken);
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
