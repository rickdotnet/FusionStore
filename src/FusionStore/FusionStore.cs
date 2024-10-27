using FusionStore.Abstractions;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;
using ZiggyCreatures.Caching.Fusion;

namespace FusionStore;

public interface IFusionStore<TKey> : IDataStore<TKey>;
public class FusionStore<TKey> : DataStore<TKey>, IFusionStore<TKey>
{
    private readonly string cacheName;
    private readonly IDataStore<TKey> innerStore;
    private readonly FusionCache cache;

    public bool SkipCache { get; set; }

    public FusionStore(IDataStore<TKey> innerStore, FusionStoreConfig config)
    {
        this.innerStore = innerStore;
        cacheName = $"FusionStore-{config.StoreName}";
        SkipCache = config.SkipCache;

        var entryOptions = config.DefaultFusionCacheEntryOptions ?? new FusionCacheEntryOptions();
        var fusionCacheOptions = new FusionCacheOptions { CacheName = cacheName, DefaultEntryOptions = entryOptions };
        cache = new FusionCache(fusionCacheOptions);
    }

    private string GetCacheKey(TKey id) => $"{cacheName}-{id}";

    public override async ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token = default)
    {
        if (SkipCache)
            return await innerStore.Get<TData>(id, token);

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

    public override async ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token = default)
    {
        var (insertResult, id) = await innerStore.Insert(data, token);
        if (SkipCache) return (insertResult, id);
        
        var result = await insertResult.SelectAsync(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, data, token: token);
            return x;
        });

        return (result, id);
    }

    public override async ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token = default)
    {
        var saveResult = await innerStore.Save(id, data, token);
        if (SkipCache) return saveResult;
        
        var result = await saveResult.SelectAsync(async x =>
        {
            var key = GetCacheKey(id);
            await cache.SetAsync(key, x, token: token);
            return x;
        });

        return result;
    }

    public override async ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token = default)
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

    public override ValueTask<Result<IEnumerable<TData>>> List<TData>(FilterCriteria<TData>? filterCriteria = null, CancellationToken token = default)
    {
        // TODO: in order to cache here, we'd need to consider the filter criteria
        return innerStore.List(filterCriteria, token);
    }

    protected override ValueTask<IEnumerable<TKey>> GetAllIds<TData>(CancellationToken token)
    {
        // won't be called, we override List
        throw new NotImplementedException();
    }
}