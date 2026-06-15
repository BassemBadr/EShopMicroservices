namespace Shopping.Web.Pages
{
    public class ProductListModel
        (ICatalogService catalogService, IBasketService basketService, ILogger<ProductListModel> logger)
        : PageModel
    {
        public IEnumerable<string> CategoryList { get; set; } = [];
        public IEnumerable<ProductModel> ProductList { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public string SelectedCategory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string categoryName)
        {
            //TODO: this method needs some enhancement in loading data like applying pagination and filtering in database not in memory
            var response = await catalogService.GetProducts();

            CategoryList = response.Products.SelectMany(a => a.Category).Distinct();

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                ProductList = response.Products.Where(p => p.Category.Contains(categoryName));
                SelectedCategory = categoryName;
            }
            else
            {
                ProductList = response.Products;
            }

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
                Color = "Black",
                ProductName = productResponse.Product.Name,
                Price = productResponse.Product.Price,
                Quantity = 1
            });

            await basketService.StoreBasket(new StoreBasketRequest(basket));

            return RedirectToPage("Cart");
        }
    }
}
