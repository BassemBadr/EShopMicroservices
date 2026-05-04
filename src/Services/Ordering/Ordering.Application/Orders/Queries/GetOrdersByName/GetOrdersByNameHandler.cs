namespace Ordering.Application.Orders.Queries.GetOrdersByName;

public class GetOrdersByNameHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
    {
        //get orders by name using dbContext
        //return result

        var orders = await dbContext.Orders.Include(o => o.OrderItems)
            .AsNoTracking()
            .Where(o => o.OrderName.Value.Contains(query.Name))
            .OrderBy(o => o.OrderName)
            .ToListAsync(cancellationToken);

        var orderDtos = OrderToOrderDto(orders);

        return new GetOrdersByNameResult(orderDtos);
    }

    private static List<OrderDto> OrderToOrderDto(List<Order> orders)
    {
        List<OrderDto> result = new();
        foreach (var order in orders)
        {
            var orderDto = new OrderDto(
                Id: order.Id.Value,
                CustomerId: order.CustomerId.Value,
                OrderName: order.OrderName.Value,
                ShippingAddress: new AddressDto(
                    FirstName: order.ShippingAddress.FirstName,
                    LastName: order.ShippingAddress.LastName,
                    EmailAddress: order.ShippingAddress.EmailAddress,
                    AddressLine: order.ShippingAddress.AddressLine,
                    Country: order.ShippingAddress.Country,
                    State: order.ShippingAddress.State,
                    ZipCode: order.ShippingAddress.ZipCode
                ),
                BillingAddress: new AddressDto(
                    FirstName: order.BillingAddress.FirstName,
                    LastName: order.BillingAddress.LastName,
                    EmailAddress: order.BillingAddress.EmailAddress,
                    AddressLine: order.BillingAddress.AddressLine,
                    Country: order.BillingAddress.Country,
                    State: order.BillingAddress.State,
                    ZipCode: order.BillingAddress.ZipCode
                ),
                Payment: new PaymentDto(
                    CardName: order.Payment.CardName,
                    CardNumber: order.Payment.CardNumber,
                    Expiration: order.Payment.Expiration,
                    Cvv: order.Payment.CVV,
                    PaymentMethod: order.Payment.PaymentMethod
                ),
                Status: order.Status,
                OrderItems: order.OrderItems.Select(oi => new OrderItemDto(
                    OrderId: oi.OrderId.Value,
                    ProductId: oi.ProductId.Value,
                    Quantity: oi.Quantity,
                    Price: oi.Price)).ToList()
                );

            result.Add(orderDto);
        }

        return result;
    }
}
