using Ordering.Application.Orders.Queries.GetOrdersByName;

namespace Ordering.API.Endpoints;

//- Accepts a name parameter
//- Constructs a GetOrdersByNameQuery with the provided parameters
//- Uses MediatR to  send the query to the corresponding handler
//- Returns matching orders based on the provided name

//public record GetOrdersByNameRequest(string Name);

public record GetOrdersByNameResponse(IEnumerable<OrderDto> Orders);

public class GetOrdersByName : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderName}", async (string orderName, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByNameQuery(orderName));

            var response = result.Adapt<GetOrdersByNameResponse>();

            return Results.Ok(response);
        })
        .WithName("GetOrdersByName")
        .Produces<GetOrdersByNameResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get orders by name")
        .WithDescription("Retrieves orders based on the provided order name.");
    }
}
