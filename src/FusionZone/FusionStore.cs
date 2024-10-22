using FusionZone.Abstractions;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;
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

    public async ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token)
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

    public async ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token)
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

    public async ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token)
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

    public async ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token)
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

    private string GetCacheKey(TKey id) => $"{cacheName}-{id}";
}