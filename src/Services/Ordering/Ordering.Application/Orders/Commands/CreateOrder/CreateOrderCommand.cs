using BuildingBlocks.CQRS;
using FluentValidation;
using Ordering.Application.Dtos;

namespace Ordering.Application.Orders.Commands.CreateOrder;


public record CreateORderCommand(OrderDto Order) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Guid Id);

public class CreateOrderCommandValidator : AbstractValidator<CreateORderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order).NotNull().WithMessage("Order cannot be null");
        RuleFor(x => x.Order.OrderName).NotNull().WithMessage("Order name cannot be null");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId cannot be null");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems cannot be null");
    }
}

