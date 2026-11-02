namespace Basket.API.Models;

public class ShoppingCart
{
    public string UserName { get; set; } = default!;
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(a => a.Price * a.Quantity);

    public ShoppingCart(string userName)
    {
        UserName = userName;
    }

    //  Required for mapping
    public ShoppingCart()
    {
    }

    /// <summary>
    /// Folds repeated lines of the same product into the first one and sums their quantities.
    /// Adding a product that is already in the cart must raise its quantity, not add a second line.
    /// </summary>
    public void MergeDuplicateItems()
    {
        Items = Items
            .GroupBy(item => item.ProductId)
            .Select(group =>
            {
                var line = group.First();
                line.Quantity = group.Sum(item => item.Quantity);
                return line;
            })
            .ToList();
    }
}
