using BuildingBlocks.Pagination;

namespace Ordering.Application.Orders.Queries.GetOrders;

public class GetOrdersHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        //get orders with pagination
        //returnr esult
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        var ordersQuery = dbContext.Orders.ForCaller(userContext);

        var totalCount = await ordersQuery.LongCountAsync(cancellationToken);

        var orders = await ordersQuery
                        .Include(o => o.OrderItems)
                        .AsNoTracking()
                        .OrderBy(o => o.OrderName.Value)
                        .Skip(pageSize * pageIndex)
                        .Take(pageSize)
                        .ToListAsync(cancellationToken);

        return new GetOrdersResult(
            new PaginatedResult<OrderDto>(
                pageIndex,
                pageSize,
                totalCount,
                orders.ToOrderDtoList()));
    }
}
