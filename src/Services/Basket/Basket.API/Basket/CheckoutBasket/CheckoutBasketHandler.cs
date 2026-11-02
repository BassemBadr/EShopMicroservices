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
        // Identity (userName + customerId) is not part of the payload — see CheckoutBasketHandler.
        RuleFor(x => x.BasketCheckoutDto).NotNull();
    }
}

public class CheckoutBasketHandler
    (IBasketRepository repository, IPublishEndpoint publishEndpoint, IUserContext userContext)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        //get existing basket with total price
        //set total price on basketcheckout event message
        //send basket checkout to rabbitmq using masstransit
        //delete the basket

        var userKey = userContext.RequireUserKey();
        var customerId = userContext.RequireUserId();

        var basket = await repository.GetBasket(userKey, cancellationToken);
        if (basket == null)
        {
            return new CheckoutBasketResult(false);
        }

        var eventMessage = MapBasketToBasketEventMessage(command, basket, userKey, customerId);

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        await repository.DeleteBasket(userKey, cancellationToken);

        return new CheckoutBasketResult(true);
    }

    private static BasketCheckoutEvent MapBasketToBasketEventMessage(
        CheckoutBasketCommand command, ShoppingCart basket, string userKey, Guid customerId)
    {
        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();

        // Identity travels to Ordering from the validated token, so downstream services never
        // have to trust what the browser said about who it is.
        eventMessage.UserName = userKey;
        eventMessage.CustomerId = customerId;

        eventMessage.TotalPrice = basket.TotalPrice;
        eventMessage.OrderItems = basket.Items.Select(item => item.Adapt<BasketCheckoutItem>()).ToList();
        return eventMessage;
    }
}
