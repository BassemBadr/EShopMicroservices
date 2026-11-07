using Shopping.Web.Models.Ordering;

namespace Shopping.Web.Pages
{
    public class OrderListModel
        (IOrderingService orderingService)
        : PageModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            // Ordering narrows the list to the caller: the customer id is read from the access token
            // on the service side, so the page never has to pass (or trust) one.
            var response = await orderingService.GetOrdersByCustomer();
            Orders = response.Orders;

            return Page();
        }
    }
}
