using BuildingBlocks.Authentication;

namespace Ordering.Application.Extensions;

public static class OrderQueryExtensions
{
    /// <summary>
    /// Narrows the query to the caller's own orders. The customer id comes from the validated token
    /// and never from the request, so one shopper can never read another's orders by guessing an id.
    /// </summary>
    /// <remarks>
    /// A <c>client_credentials</c> token has no customer identity at all; it is the machine/service
    /// view used by tooling (the Postman collection), so it keeps seeing every order.
    /// </remarks>
    public static IQueryable<Order> ForCaller(this IQueryable<Order> orders, IUserContext userContext)
    {
        if (userContext.UserId is not { } callerId)
        {
            return orders;
        }

        var customerId = CustomerId.Of(callerId);

        return orders.Where(order => order.CustomerId == customerId);
    }
}
