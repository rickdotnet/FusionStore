using FusionZone.Stores.ZoneTree.Internal;
using Tenray.ZoneTree;

namespace FusionZone.Stores.ZoneTree;

public class FuzionZoneTreeFactory
{
    internal static IZoneTree<TKey, TypedDeletable> Create<TKey>(ZoneStoreConfig storeConfig)
    {
        var dataDirectory = Path.Combine(storeConfig.DataPath, storeConfig.StoreName);
        return new ZoneTreeFactory<TKey, TypedDeletable>()
            .SetDataDirectory(dataDirectory)
            .SetIsDeletedDelegate((in TKey _, in TypedDeletable value) => value.IsDeleted)
            .SetMarkValueDeletedDelegate((ref TypedDeletable value) => value.IsDeleted = true)
            .SetComparer(ZoneTreeRegistry.Default.GetComparer<TKey>())
            .SetKeySerializer(ZoneTreeRegistry.Default.GetSerializer<TKey>() ?? new DefaultZoneSerializer<TKey>())
            .SetValueSerializer(ZoneTreeRegistry.Default.GetSerializer<TypedDeletable>() ?? new DefaultZoneSerializer<TypedDeletable>())
            .OpenOrCreate();
    }

    public static IZoneTree<TKey, TValue> Create<TKey, TValue>(ZoneStoreConfig storeConfig)
    {
        var dataDirectory = Path.Combine(storeConfig.DataPath, storeConfig.StoreName);
        return new ZoneTreeFactory<TKey, TValue>()
            .SetComparer(ZoneTreeRegistry.Default.GetComparer<TKey>())
            .SetDataDirectory(dataDirectory)
            .SetKeySerializer(ZoneTreeRegistry.Default.GetSerializer<TKey>() ?? new DefaultZoneSerializer<TKey>())
            .SetValueSerializer(ZoneTreeRegistry.Default.GetSerializer<TValue>() ?? new DefaultZoneSerializer<TValue>())
            .OpenOrCreate();
    }
}