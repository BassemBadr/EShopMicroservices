using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler : ICommandHandler<CreateORderCommand, CreateOrderResult>
{
    public Task<CreateOrderResult> Handle(CreateORderCommand request, CancellationToken cancellationToken)
    {
        //create order entity from command object
        //save to database
        //return result with order id
        throw new NotImplementedException();
    }
}
