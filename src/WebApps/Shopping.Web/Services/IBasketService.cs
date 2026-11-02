using System.Net;

namespace Shopping.Web.Services;

public interface IBasketService
{
    // The basket API takes the owner from the access token, so the client never names a user.
    [Get("/basket-service/basket")]
    Task<GetBasketResponse> GetBasket();

    [Post("/basket-service/basket")]
    Task<StoreBasketResponse> StoreBasket(StoreBasketRequest request);

    [Delete("/basket-service/basket")]
    Task<DeleteBasketResponse> DeleteBasket();

    [Post("/basket-service/basket/checkout")]
    Task<CheckoutBasketResponse> CheckoutBasket(CheckoutBasketRequest request);

    public async Task<ShoppingCartModel> LoadUserBasket()
    {
        try
        {
            var getBasketResponse = await GetBasket();
            return getBasketResponse.Cart;
        }
        catch (ApiException apiException) when (apiException.StatusCode == HttpStatusCode.NotFound)
        {
            // No basket stored for this caller yet — start from an empty local cart.
            return new ShoppingCartModel { Items = [] };
        }
    }
}
