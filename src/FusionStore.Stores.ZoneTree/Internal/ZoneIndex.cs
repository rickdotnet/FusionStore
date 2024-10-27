using RickDotNet.Base;
using Tenray.ZoneTree;

namespace FusionStore.Stores.ZoneTree.Internal;

internal abstract class ZoneIndex
{
    public static ZoneIndex<TKey> Create<TKey>(ZoneStoreConfig storeConfig) => new(storeConfig);
}

internal class ZoneIndex<TKey>
{
    private readonly IZoneTree<string, HashSet<TKey>> indexStore;

    public ZoneIndex(ZoneStoreConfig storeConfig)
    {
        var indexConfig = storeConfig with { StoreName = $"{storeConfig.StoreName}-Index" };
        indexStore = FuzionZoneTreeFactory.Create<string, HashSet<TKey>>(indexConfig);
    }

    private string GetIndexKey<TData>() => typeof(TData).FullName!;

    public Result<TKey> Add<TData>(TKey key)
    {
        var indexKey = GetIndexKey<TData>();
        if (!indexStore.TryGet(indexKey, out var idSet)) 
            idSet = [];

        idSet.Add(key);
        indexStore.Upsert(indexKey, idSet);

        return key;
    }

    public Result<TKey> Remove<TData>(TKey key)
    {
        var indexKey = GetIndexKey<TData>();

        if (!indexStore.TryGet(indexKey, out var idSet))
            return Result.Failure<TKey>($"Item not found: {key?.ToString()}");

        var success = idSet != null && idSet.Remove(key);
        return success
            ? key // success 
            : Result.Failure<TKey>($"Item not found: {key?.ToString()}");
    }

    public IEnumerable<TKey> GetAllIds<TData>()
    {
        var indexKey = GetIndexKey<TData>();

        if (indexStore.TryGet(indexKey, out var idSet))
            return idSet ?? [];

        return [];
    }
}