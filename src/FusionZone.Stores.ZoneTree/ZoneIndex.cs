using RickDotNet.Base;
using Tenray.ZoneTree;

namespace FusionZone.Stores.ZoneTree;

public abstract class ZoneIndex
{
    public static ZoneIndex<TKey> Create<TKey>(ZoneStoreConfig storeConfig) => new(storeConfig);
}

public class ZoneIndex<TKey>
{
    private readonly ZoneStoreConfig storeConfig;
    private readonly IZoneTree<string, HashSet<TKey>> indexStore;

    public ZoneIndex(ZoneStoreConfig storeConfig)
    {
        this.storeConfig = storeConfig with { StoreName = $"{storeConfig.StoreName}-Index" };
        indexStore = ZoneTreeFactory.Create<string, HashSet<TKey>>(storeConfig);
    }

    public string GetIndexKey<TData>() => typeof(TData).FullName!;

    public Result<TKey> Add<TData>(TKey key)
    {
        var indexKey = GetIndexKey<TData>();
        // get the set of keys
        // upsert the key to the set
        // save the set back
        return key;
    }

    public Result<TKey> Remove<TData>(TKey key)
    {
        var indexKey = GetIndexKey<TData>();
        // get the set of keys
        // remove the key from the set
        // save the set back
        return key;
    }
    
    public IEnumerable<TKey> GetAllIds<TData>(CancellationToken token)
    {
        var indexKey = GetIndexKey<TData>();
        // get the set of keys
        // return the set as IEnumerable
        throw new NotImplementedException();
    }
}