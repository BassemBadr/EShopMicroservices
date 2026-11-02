namespace Shopping.Web.Models.Basket;

/// <summary>
/// What the browser may send at checkout. It has no user name or customer id: the basket API
/// takes both from the caller's access token before publishing the checkout event.
/// </summary>
public class BasketCheckoutModel
{
    public decimal TotalPrice { get; set; } = default!;

    // Shipping and BillingAddress
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

    // Payment
    public string CardName { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public string Expiration { get; set; } = default!;
    public string CVV { get; set; } = default!;
    public int PaymentMethod { get; set; } = default!;
}

//wrapper class
public record CheckoutBasketRequest(BasketCheckoutModel BasketCheckoutDto);//must keep same names from gateway

public record CheckoutBasketResponse(bool IsSuccess);
