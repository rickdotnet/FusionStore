using RickDotNet.Base;

namespace FusionStore.Abstractions;

public interface IDataStore<TKey> 
{
    ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token);
    ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token);
    ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token);
    ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token);

    ValueTask<Result<IEnumerable<TData>>> List<TData>(
        FilterCriteria<TData>? filterCriteria = null,
        CancellationToken token = default);
}