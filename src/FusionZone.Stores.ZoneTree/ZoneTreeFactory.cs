using Tenray.ZoneTree;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;

namespace FusionZone.Stores.ZoneTree;

public class ZoneTreeFactory
{
    public static IZoneTree<TKey, string> Create<TKey>(ZoneStoreConfig storeConfig)
        => Create<TKey, string>(storeConfig);

    public static IZoneTree<TKey, TValue> Create<TKey, TValue>(ZoneStoreConfig storeConfig)
    {
        // need to figure out how to get comparer and serializer from the type

        var dataDirectory = Path.Combine(storeConfig.DataPath, storeConfig.StoreName);
        return new ZoneTreeFactory<TKey, TValue>()
            .SetComparer(GetComparer<TKey>())
            .SetDataDirectory(dataDirectory)
            .SetKeySerializer(GetKeySerializer<TKey>())
            .SetValueSerializer(GetValueSerializer<TValue>())
            .OpenOrCreate();
    }

    // TODO
    private static IRefComparer<TKey> GetComparer<TKey>()
    {
        throw new NotImplementedException();
    }

    private static ISerializer<TKey> GetKeySerializer<TKey>()
    {
        throw new NotImplementedException();
    }

    private static ISerializer<TValue> GetValueSerializer<TValue>()
    {
        throw new NotImplementedException();
    }
}