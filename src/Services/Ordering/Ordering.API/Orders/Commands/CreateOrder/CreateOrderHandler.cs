using BuildingBlocks.CQRS;

namespace Ordering.API.Orders.Commands.CreateOrder;

public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        //create order entity from command object
        //save to database
        //return result
        throw new NotImplementedException();
    }
}
