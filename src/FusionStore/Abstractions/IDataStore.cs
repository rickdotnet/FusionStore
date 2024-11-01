using RickDotNet.Base;

namespace FusionStore.Abstractions;

public interface IDataStore<TKey> 
{
    ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token = default);
    ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token = default);
    ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token = default);
    ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token = default);
    ValueTask<Result<IEnumerable<TData>>> List<TData>(
        FilterCriteria<TData>? filterCriteria = null,
        CancellationToken token = default);
}