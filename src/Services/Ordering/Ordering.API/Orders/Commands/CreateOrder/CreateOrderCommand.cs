using BuildingBlocks.CQRS;
using FluentValidation;
using Ordering.API.Dtos;

namespace Ordering.API.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderDto Order) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Guid Id);

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order).NotNull();
        RuleFor(x => x.Order.CustomerId).NotEmpty();
        RuleFor(x => x.Order.OrderName).NotEmpty();
        RuleFor(x => x.Order.OrderItems).NotEmpty();
    }
}