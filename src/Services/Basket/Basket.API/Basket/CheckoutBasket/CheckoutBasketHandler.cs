using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Basket.API.Basket.CheckoutBasket;

public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto)
    : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.BasketCheckoutDto).NotNull();
        RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty();
    }
}

public class CheckoutBasketHandler
    (IBasketRepository repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        //get existing basket with total price
        //set total price on basketcheckout event message
        //send basket checkout to rabbitmq using masstransit
        //delete the basket

        var basket = await repository.GetBasket(command.BasketCheckoutDto.UserName, cancellationToken);
        if (basket == null)
        {
            return new CheckoutBasketResult(false);
        }

        var eventMessage = MapBasketToBasketEventMessage(command, basket);

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        await repository.DeleteBasket(command.BasketCheckoutDto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }

    private static BasketCheckoutEvent MapBasketToBasketEventMessage(CheckoutBasketCommand command, ShoppingCart basket)
    {
        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.TotalPrice;
        eventMessage.OrderItems = basket.Items.Select(item => item.Adapt<BasketCheckoutItem>()).ToList();
        return eventMessage;
    }
}
