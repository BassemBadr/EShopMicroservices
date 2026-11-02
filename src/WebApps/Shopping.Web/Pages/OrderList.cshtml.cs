using Shopping.Web.Models.Ordering;
using System.Security.Claims;

namespace Shopping.Web.Pages
{
    public class OrderListModel
        (IOrderingService orderingService, ILogger<OrderListModel> logger)
        : PageModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            // The 'sub' claim is the IdentityServer user id (a GUID) and is the same value Ordering
            // stores as the customer id, so each shopper only ever sees their own orders.
            if (!Guid.TryParse(User.FindFirstValue("sub"), out var customerId))
            {
                throw new InvalidOperationException(
                    "The signed-in principal has no usable 'sub' claim; check MapInboundClaims on the OIDC options.");
            }

            var response = await orderingService.GetOrdersByCustomer(customerId);
            Orders = response.Orders;

            return Page();
        }
    }
}
