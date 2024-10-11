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

    public async ValueTask<IStoreResult<T>> Get(long id, CancellationToken token)
    {
        var key = $"{cacheName}-{id}";
        var cacheHit = await cache.TryGetAsync<T>(key, token: token);
        if (cacheHit.HasValue)
            return StoreResult<T>.Success(id, cacheHit.Value);
        
        var result = await innerStore.Get(id, token);
        return !result.Succeeded 
            ? result 
            : StoreResult<T>.Success(id, result.Value);
    }

    // I would not consider this method safe
    // there is no protection against memory exhaustion
    public ValueTask<IEnumerable<IStoreResult<T>>> Get(long[] ids, CancellationToken token)
    {
        var results = GetManyInternal(ids);
        return ValueTask.FromResult(results);
    }
    
    private IEnumerable<IStoreResult<T>> GetManyInternal(IEnumerable<long> ids)
    {
        foreach (var id in ids)
        {
            var key = $"{cacheName}-{id}";
            
            var cacheHit = cache.TryGet<T>(key);
            if (cacheHit.HasValue)
                yield return StoreResult<T>.Success(id, cacheHit);
            else
                yield return StoreResult<T>.Fail(id, "Item not found");
        }
    }


    public async ValueTask<IStoreResult<T>> Insert(T data, CancellationToken token)
    {
        var result = await innerStore.Insert(data, token);
        if (!result.Succeeded) return result;

        var key = $"{cacheName}-{result.Id}";
        await cache.SetAsync(key, data, token: token);

        return StoreResult<T>.Success(result.Id, data);
    }

    public async ValueTask<IStoreResult<T>> Save(long id, T data, CancellationToken token)
    {
        var result = await innerStore.Save(id, data, token);
        if (!result.Succeeded) return result;

        var key = $"{cacheName}-{id}";
        await cache.SetAsync(key, data, token: token);

        return StoreResult<T>.Success(id, data);
    }

    public async ValueTask<IStoreResult<T>> Delete(long id, CancellationToken token)
    {
        var result = await innerStore.Delete(id, token);
        if (!result.Succeeded) return result;

        var key = $"{cacheName}-{id}";
        await cache.RemoveAsync(key, token: token);

        return result;
    }
}