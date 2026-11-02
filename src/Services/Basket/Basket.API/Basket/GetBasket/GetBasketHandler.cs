namespace Basket.API.Basket.GetBasket;

public record GetBasketQuery : IQuery<GetBasketResult>;

public record GetBasketResult(ShoppingCart Cart);

internal class GetBasketQueryHandler
    (IBasketRepository repository, IUserContext userContext) : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(userContext.RequireUserKey(), cancellationToken);

        return new GetBasketResult(basket);
    }
}
