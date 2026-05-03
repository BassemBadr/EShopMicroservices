using Ordering.Domain.Enums;

namespace Ordering.API.Dtos;

public record OrderDto
(
    Guid Id,
    Guid CustomerId,
    string OrderName,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    PaymentDto Payment,
    OrderStatus Status,
    List<OrderItemDto> OrderItems
    );
