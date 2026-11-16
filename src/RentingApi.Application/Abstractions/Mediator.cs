using System;
using System.Threading;
using System.Threading.Tasks;

namespace RentingApi.Application.Abstractions;

/// <summary>
/// Custom mediator/dispatcher for CQRS command and query dispatching.
/// Resolves handlers from the DI container.
/// </summary>
public sealed class Mediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Sends a command with a result.
    /// </summary>
    public async Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handlerType = typeof(ICommandHandler<TCommand, TResult>);
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for command '{typeof(TCommand).Name}'. " +
                $"Ensure '{handlerType.Name}' is registered in DI and the handler is internal with [InternalsVisibleTo].");
        }

        return await ((ICommandHandler<TCommand, TResult>)handler).Handle(command, cancellationToken);
    }

    /// <summary>
    /// Sends a command without a result.
    /// </summary>
    public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handlerType = typeof(ICommandHandler<TCommand>);
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for command '{typeof(TCommand).Name}'. " +
                $"Ensure '{handlerType.Name}' is registered in DI and the handler is internal with [InternalsVisibleTo].");
        }

        await ((ICommandHandler<TCommand>)handler).Handle(command, cancellationToken);
    }

    /// <summary>
    /// Sends a query.
    /// </summary>
    public async Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handlerType = typeof(IQueryHandler<TQuery, TResult>);
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for query '{typeof(TQuery).Name}'. " +
                $"Ensure '{handlerType.Name}' is registered in DI and the handler is internal.");
        }

        return await ((IQueryHandler<TQuery, TResult>)handler).Handle(query, cancellationToken);
    }
}
