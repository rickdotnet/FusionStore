using FusionZone.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace FusionZone;

public class FusionStore<T> : IDataStore<T>
{
    private readonly string cacheName = $"FusionStore-{typeof(T).FullName}";
    private readonly IDataStore<T> innerStore;
    private readonly IFusionCache cache;

    public FusionStore(IDataStore<T> innerStore, IFusionCache cache)
    {
        this.innerStore = innerStore;
        this.cache = cache;
    }

    public async ValueTask<StoreResult<T>> Get(long id, CancellationToken token)
    {
        var key = GetCacheKey(id);
        var cacheHit = await cache.TryGetAsync<T>(key, token: token);
        if (cacheHit.HasValue)
            return StoreResult.Success(cacheHit.Value);

        var storeResult = await innerStore.Get(id, token);
        return await storeResult.Select(async x =>
        {
            await cache.SetAsync(key, x, token: token);
            return x;
        });
    }

    public async ValueTask<(StoreResult<T> result, long id)> Insert(T data, CancellationToken token)
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

    public async ValueTask<StoreResult<T>> Save(long id, T data, CancellationToken token)
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

    public async ValueTask<StoreResult<T>> Delete(long id, CancellationToken token)
    {
        var deleteResult = await innerStore.Delete(id, token);
        var result = await deleteResult.Select(async x =>
        {
            var key = GetCacheKey(id);
            await cache.RemoveAsync(key, token: token);
            return x;
        });

        return result;
    }

    private string GetCacheKey(long id) => $"{cacheName}-{id}";
}