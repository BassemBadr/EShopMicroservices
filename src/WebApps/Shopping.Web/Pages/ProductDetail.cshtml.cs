namespace Shopping.Web.Pages
{
    public class ProductDetailModel
        (ICatalogService catalogService, IBasketService basketService, ILogger<ProductDetailModel> logger)
        : PageModel
    {
        public ProductModel Product { get; set; } = default;

        [BindProperty]
        public string Color { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid productId)
        {
            var response = await catalogService.GetProduct(productId);
            Product = response.Product;
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId)
        {
            logger.LogInformation("Add to cart button clicked");

            var productResponse = await catalogService.GetProduct(productId);

            var basket = await basketService.LoadUserBasket();

            //TODO: check product if exist increase quantity if not add new line
            basket.Items.Add(new ShoppingCartItemModel
            {
                ProductId = productId,
                //TODO: add product color to product model
                Color = Color,
                ProductName = productResponse.Product.Name,
                Price = productResponse.Product.Price,
                Quantity = Quantity
            });

            await basketService.StoreBasket(new StoreBasketRequest(basket));

            return RedirectToPage("Cart");
        }
    }
}
