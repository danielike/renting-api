using System.Threading;
using System.Threading.Tasks;

namespace RentingApi.Application.Abstractions;

/// <summary>
/// Handler interface for queries.
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
