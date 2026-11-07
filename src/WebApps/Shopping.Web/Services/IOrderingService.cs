using Shopping.Web.Models.Ordering;

namespace Shopping.Web.Services;

public interface IOrderingService
{
    [Get("/ordering-service/orders?pageIndex={pageIndex}&pageSize={pageSize}")]
    Task<GetOrdersResponse> GetOrders(int? pageIndex = 1, int? pageSize = 10);

    // No customer id: Ordering takes the caller from the access token.
    [Get("/ordering-service/orders/customer")]
    Task<GetOrdersByCustomerResponse> GetOrdersByCustomer();
}
