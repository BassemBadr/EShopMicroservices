using Ordering.Application.Orders.Commands.UpdateOrder;

namespace Ordering.API.Endpoints;

//- Accepts a UpdateOrderRequest object
//- Maps the request to a UpdateOrderCommand
//- Uses MediatR to  send the command to the corresponding handler
//- Returns a success or error response based on the outcome of the update operation

public record UpdateOrderRequest(OrderDto Order);

public record UpdateOrderResponse(bool IsSuccess);

public class UpdateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/orders", async (UpdateOrderRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateOrderCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<UpdateOrderResponse>();

            return Results.Ok(response);
        })
        .WithName("UpdateOrder")
        .Produces<UpdateOrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Update order")
        .WithDescription("Update order.");
    }
}
