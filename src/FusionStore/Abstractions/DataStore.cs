using RickDotNet.Base;
using RickDotNet.Extensions.Base;

namespace FusionStore.Abstractions;

public abstract class DataStore<TKey> : IDataStore<TKey>
{
    public abstract ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token = default);
    public abstract ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token = default);

    public abstract ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token = default);
    public abstract ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token = default);

    public virtual async ValueTask<Result<IEnumerable<TData>>> List<TData>(
        FilterCriteria<TData>? filterCriteria = null,
        CancellationToken token = default)
    {
        var ids = await GetAllIds<TData>(token);
        var results = new List<TData>();
        filterCriteria ??= FilterCriteria.For<TData>();
        var compiledFilter = filterCriteria.Filter?.Compile(); // TODO: cache this

        var skipped = 0;
        var taken = 0;
        foreach (var id in ids)
        {
            var result = await Get<TData>(id, token);
            if (!result) continue; // skip if not found, for now

            var item = result.ValueOrDefault()!;

            // no filter, or filter passes
            if (compiledFilter == null || compiledFilter(item))
            {
                // apply skip
                if (skipped < filterCriteria.SkipValue)
                {
                    skipped++;
                    continue;
                }

                // apply take
                if (taken < filterCriteria.TakeValue)
                {
                    results.Add(item);
                    taken++;
                }
                else
                {
                    break; // we've taken enough, stop looping.
                }
            }
        }

        return Result.Success(results.AsEnumerable());
    }

    protected abstract ValueTask<IEnumerable<TKey>> GetAllIds<TData>(CancellationToken token);
}