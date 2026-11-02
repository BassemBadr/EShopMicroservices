namespace Basket.API.Basket.DeleteBasket;

public record DeleteBasketCommand : ICommand<DeleteBasketResult>;

public record DeleteBasketResult(bool IsSuccess);

internal class DeleteBasketCommandHandler
    (IBasketRepository repository, IUserContext userContext) : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
    {
        //TODO: delete basket from database and cache
        await repository.DeleteBasket(userContext.RequireUserKey(), cancellationToken);

        return new DeleteBasketResult(true);
    }
}
