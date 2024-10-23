using FusionZone.Abstractions;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;
using ZiggyCreatures.Caching.Fusion;

namespace FusionZone;

public class FusionStore<TKey> : DataStore<TKey>
{
    private readonly string cacheName = $"FusionStore-{typeof(TKey).FullName}";
    private readonly IDataStore<TKey> innerStore;
    private readonly IFusionCache cache;

    public FusionStore(IDataStore<TKey> innerStore, IFusionCache cache)
    {
        this.innerStore = innerStore;
        this.cache = cache;
    }

    private string GetCacheKey(TKey id) => $"{cacheName}-{id}";
    
    public override async ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token)
    {
        var key = GetCacheKey(id);
        var cacheHit = await cache.TryGetAsync<TData>(key, token: token);
        if (cacheHit.HasValue)
            return Result.Success(cacheHit.Value);

        var result = await innerStore.Get<TData>(id, token);
        return await result.SelectAsync(async x =>
        {
            await cache.SetAsync(key, x, token: token);
            return x;
        });
    }

    public override async ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token)
    {
        var (insertResult, id) = await innerStore.Insert(data, token);
        var result = await insertResult.SelectAsync(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, data, token: token);
            return x;
        });

        return (result, id);
    }

    public override async ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token)
    {
        var saveResult = await innerStore.Save(id, data, token);
        var result = await saveResult.SelectAsync(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, x, token: token);
            return x;
        });

        return result;
    }

    public override async ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token)
    {
        var deleteResult = await innerStore.Delete<TData>(id, token);
        var result = await deleteResult.SelectAsync(async x =>
        {
            var key = GetCacheKey(id);
            await cache.RemoveAsync(key, token: token);
            return x;
        });

        return result;
    }

    public override ValueTask<Result<IEnumerable<TData>>> List<TData>(FilterCriteria<TData> filterCriteria, CancellationToken token = default)
    {
        // TODO: decide if we want to cache this
        return innerStore.List(filterCriteria, token);
    }

    protected override Task<IEnumerable<TKey>> GetAllIdsAsync<TData>(CancellationToken token)
    {
        // won't be called, we override List
        throw new NotImplementedException();
    }
}