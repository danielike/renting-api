namespace RentingApi.Application.Abstractions;

/// <summary>
/// Handler interface for queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
