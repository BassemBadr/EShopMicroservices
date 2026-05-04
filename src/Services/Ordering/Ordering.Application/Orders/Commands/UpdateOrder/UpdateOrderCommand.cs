using FluentValidation;

namespace Ordering.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(OrderDto Order) : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Order).NotNull().WithMessage("Order cannot be null");
        RuleFor(x => x.Order.Id).NotNull().WithMessage("Order Id cannot be null");
        RuleFor(x => x.Order.OrderName).NotNull().WithMessage("Order name cannot be null");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId cannot be null");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems cannot be null");
    }
}