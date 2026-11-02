namespace Shopping.Web.Pages
{
    public class CheckOutModel
        (IBasketService basketService, ILogger<CheckOutModel> logger)
        : PageModel
    {
        [BindProperty]
        public BasketCheckoutModel Order { get; set; } = default!;

        public ShoppingCartModel Cart { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await basketService.LoadUserBasket();

            // Preselect credit card (1 = Credit card in the markup). The checked state is now driven
            // by the model, so a failed POST keeps whatever the user actually picked.
            Order = new BasketCheckoutModel { PaymentMethod = 1 };

            return Page();
        }

        public async Task<IActionResult> OnPostCheckOutAsync()
        {
            logger.LogInformation("checkout button clicked");

            Cart = await basketService.LoadUserBasket();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // UserName and CustomerId are not sent from here any more: the basket API stamps both
            // from the caller's access token before publishing the checkout event to Ordering.
            Order.TotalPrice = Cart.TotalPrice;

            await basketService.CheckoutBasket(new CheckoutBasketRequest(Order));

            return RedirectToPage("Confirmation", "OrderSubmitted");
        }
    }
}
