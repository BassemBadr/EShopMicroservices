namespace Shopping.Web.Pages
{
    public class CartModel
        (IBasketService basketService, ILogger<CartModel> logger)
        : PageModel
    {
        public ShoppingCartModel Cart { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await basketService.LoadUserBasket();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(Guid productId, int quantity)
        {
            logger.LogInformation("Update quantity clicked for {ProductId}: {Quantity}", productId, quantity);

            // min/max on the input are client-side only, so the stored cart never trusts them.
            if (quantity < 1)
            {
                return RedirectToPage();
            }

            var cart = await basketService.LoadUserBasket();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item is not null)
            {
                item.Quantity = quantity;
                await basketService.StoreBasket(new StoreBasketRequest(cart));
            }

            // Redirect after post so a refresh cannot replay the old quantity.
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveFromCartAsync(Guid productId)
        {
            logger.LogInformation("Remove from cart button is clicked");
            Cart = await basketService.LoadUserBasket();

            Cart.Items.RemoveAll(x => x.ProductId == productId);

            await basketService.StoreBasket(new StoreBasketRequest(Cart));

            return RedirectToPage();
        }
    }
}
