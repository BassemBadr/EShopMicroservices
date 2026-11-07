namespace Ordering.Application.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        //get the caller's orders using dbcontext
        //return result

        var orders = await dbContext.Orders
            .ForCaller(userContext)
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .OrderBy(o => o.OrderName.Value)
            .ToListAsync(cancellationToken);

        return new GetOrdersByCustomerResult(orders.ToOrderDtoList());
    }
}
