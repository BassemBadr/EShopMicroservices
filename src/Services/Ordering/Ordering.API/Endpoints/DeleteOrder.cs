using Ordering.Application.Orders.Commands.DeleteOrder;

namespace Ordering.API.Endpoints;

//- Accepts the order ID as a parameter
//- Constructs a DeleteOrderCommand
//- Sends the command using MediatR
//- Returns success or not found response

//public record DeleteOrderRequest(Guid Id);

public record DeleteOrderResponse(bool IsSuccess);

public class DeleteOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/orders/{id}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteOrderCommand(id);
            
            var result = await sender.Send(command);
            
            var response = result.Adapt<DeleteOrderResponse>();
            
            return Results.Ok(response);
        })
        .WithName("DeleteOrder")
        .Produces<DeleteOrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Delete order")
        .WithDescription("Deletes an existing order based on the provided order ID.");
    }
}
