using System.Threading;
using System.Threading.Tasks;

namespace RentingApi.Application.Abstractions;

/// <summary>
/// Handler interface for commands with a result.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handler interface for commands without a result.
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken cancellationToken);
}
