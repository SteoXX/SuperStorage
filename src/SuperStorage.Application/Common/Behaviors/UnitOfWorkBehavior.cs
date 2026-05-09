using MediatR;
using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Application.Abstractions.Persistence;

namespace SuperStorage.Application.Common.Behaviors;

/// <summary>
///     A MediatR pipeline behavior that manages a unit of work for commands.
/// 
///     It begins a transaction before the command is handled, commits the transaction if the command is successful,
///     and rolls back the transaction if an exception occurs.
/// </summary>
internal sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply the unit of work behavior to commands, not queries.
        if (request is not ICommand<TResponse> && request is not ICommand)
        {
            return await next(cancellationToken);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
