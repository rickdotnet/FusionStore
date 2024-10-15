using FusionZone.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace FusionZone;

public class FusionStore<TKey> : IDataStore<TKey>
{
    private readonly string cacheName = $"FusionStore-{typeof(TKey).FullName}";
    private readonly IDataStore<TKey> innerStore;
    private readonly IFusionCache cache;

    public FusionStore(IDataStore<TKey> innerStore, IFusionCache cache)
    {
        this.innerStore = innerStore;
        this.cache = cache;
    }

    public async ValueTask<StoreResult<TData>> Get<TData>(TKey id, CancellationToken token)
    {
        var key = GetCacheKey(id);
        var cacheHit = await cache.TryGetAsync<TData>(key, token: token);
        if (cacheHit.HasValue)
            return StoreResult.Success(cacheHit.Value);

        var storeResult = await innerStore.Get<TData>(id, token);
        return await storeResult.Select(async x =>
        {
            await cache.SetAsync(key, x, token: token);
            return x;
        });
    }

    public async ValueTask<(StoreResult<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token)
    {
        var (insertResult, id) = await innerStore.Insert(data, token);
        var result = await insertResult.Select(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, data, token: token);
            return x;
        });

        return (result, id);
    }

    public async ValueTask<StoreResult<TData>> Save<TData>(TKey id, TData data, CancellationToken token)
    {
        var saveResult = await innerStore.Save(id, data, token);
        var result = await saveResult.Select(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, x, token: token);
            return x;
        });

        return result;
    }

    public async ValueTask<StoreResult<TData>> Delete<TData>(TKey id, CancellationToken token)
    {
        var deleteResult = await innerStore.Delete<TData>(id, token);
        var result = await deleteResult.Select(async x =>
        {
            var key = GetCacheKey(id);
            await cache.RemoveAsync(key, token: token);
            return x;
        });

        return result;
    }

    private string GetCacheKey(TKey id) => $"{cacheName}-{id}";
}