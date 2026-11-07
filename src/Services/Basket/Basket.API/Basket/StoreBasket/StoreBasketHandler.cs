using Discount.Grpc.Protos;

namespace Basket.API.Basket.StoreBasket;

public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;

public record StoreBasketResult(string UserName);

public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        // No UserName rule: the owner is stamped from the access token by the handler.
        RuleFor(x => x.Cart).NotNull().WithMessage("cart can not be null");
        RuleFor(x => x.Cart.Items).NotNull();
    }
}

internal class StoreBasketCommandHandler
    (IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProto, IUserContext userContext)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        // The basket always belongs to the caller — never to a name supplied in the body.
        command.Cart.UserName = userContext.RequireUserKey();

        // One line per product: adding the same product again only raises its quantity.
        command.Cart.MergeDuplicateItems();

        var storedCart = await LoadStoredCart(command.Cart.UserName, cancellationToken);

        await DeductDiscount(command.Cart, storedCart, cancellationToken);

        await repository.StoreBasket(command.Cart, cancellationToken);

        return new StoreBasketResult(command.Cart.UserName);
    }

    /// <summary>The store throws when the caller has no basket yet; that is a normal first store.</summary>
    private async Task<ShoppingCart?> LoadStoredCart(string userName, CancellationToken cancellationToken)
    {
        try
        {
            return await repository.GetBasket(userName, cancellationToken);
        }
        catch (BasketNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Applies the product discount once per line. The client posts the whole cart back on every
    /// change (add, remove, quantity), so a line that is already stored keeps its stored price —
    /// re-deducting would shave the discount off it again on each edit.
    /// </summary>
    private async Task DeductDiscount(ShoppingCart cart, ShoppingCart? storedCart, CancellationToken cancellationToken)
    {
        //  Communicate with discount gRPC service to get discount for each item in the cart and deduct it from the price of the item
        foreach (var item in cart.Items)
        {
            var storedItem = storedCart?.Items.FirstOrDefault(stored => stored.ProductId == item.ProductId);

            if (storedItem is not null)
            {
                item.Price = storedItem.Price;
                continue;
            }

            var discount = await discountProto.GetDiscountAsync(new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: cancellationToken);

            // A discount bigger than the price must never turn a line negative.
            item.Price = Math.Max(0, item.Price - discount.Amount);
        }
    }
}
