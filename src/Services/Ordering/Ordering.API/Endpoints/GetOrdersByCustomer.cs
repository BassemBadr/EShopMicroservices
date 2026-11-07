using Ordering.Application.Orders.Queries.GetOrdersByCustomer;

namespace Ordering.API.Endpoints;

//- Takes the caller's identity from the validated access token
//- Constructs a GetOrdersByCustomerQuery (no customer Id in the route)
//- Uses MediatR to  send the query to the corresponding handler
//- Returns the caller's orders

//public record GetOrdersByCustomerRequest(Guid CustomerId);

public record GetOrdersByCustomerResponse(IEnumerable<OrderDto> Orders);

public class GetOrdersByCustomer : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/customer", async (ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery());
            
            var response = result.Adapt<GetOrdersByCustomerResponse>();
            
            return Results.Ok(response);
        })
        .WithName("GetOrdersByCustomer")
        .Produces<GetOrdersByCustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get orders by customer")
        .WithDescription("Retrieves the caller's own orders; the customer is taken from the access token, not the URL.");
    }
}
