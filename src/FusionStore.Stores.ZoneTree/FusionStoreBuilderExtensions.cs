using IdGen;

namespace FusionStore.Stores.ZoneTree;

public static class FusionStoreBuilderExtensions
{
    // placeholder until I figure out this the DX around IdGeneration
    // until then, we only really support longs
    public static IFusionStoreBuilder<long> WithZoneTree(this IFusionStoreBuilder<long> builder, string dataPath)
    {
        var zoneStoreConfig =  new ZoneStoreConfig
        {
            StoreName = builder.Config.StoreName,
            DataPath = dataPath
        };
        
        var store = new ZoneStore<long>(zoneStoreConfig, new IdGenerator(0));
        builder.WithInnerStore(store);
        return builder;
    }
}