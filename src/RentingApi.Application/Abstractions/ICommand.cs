namespace RentingApi.Application.Abstractions;

/// <summary>
/// Base interface for commands (mutable operations that change state).
/// </summary>
public interface ICommand<TResult>
{
}

/// <summary>
/// Base interface for commands without a result.
/// </summary>
public interface ICommand
{
}
