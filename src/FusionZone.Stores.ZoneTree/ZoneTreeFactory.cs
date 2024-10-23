using Tenray.ZoneTree;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;

namespace FusionZone.Stores.ZoneTree;

public class ZoneTreeFactory
{
    public static IZoneTree<TKey,string> Create<TKey>(ZoneStoreConfig storeConfig)
    {
        // need to figure out how to get comparer and serializer from the type
        
        var dataDirectory = Path.Combine(storeConfig.DataPath, storeConfig.StoreName);
        return new ZoneTreeFactory<TKey, string>()
            .SetComparer(GetComparer<TKey>())
            .SetDataDirectory(dataDirectory)
            .SetKeySerializer(GetKeySerializer<TKey>())
            .SetValueSerializer(new Utf8StringSerializer())
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
}