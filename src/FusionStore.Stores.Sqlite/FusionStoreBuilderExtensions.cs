namespace FusionStore.Stores.Sqlite;

public static class FusionStoreBuilderExtensions
{
    // placeholder until I figure out this the DX around IdGeneration
    // until then, we only really support longs
    public static IFusionStoreBuilder<long> WithSqlite(this IFusionStoreBuilder<long> builder, string dataPath)
    {
        var storeConfig =  new SqliteStoreConfig
        {
            StoreName = builder.Config.StoreName,
            DataPath = dataPath
        };
        
        var store = new SqliteStore<long>(storeConfig);
        store.Initialize();
        
        builder.WithInnerStore(store);
        return builder;
    }
}

public record SqliteStoreConfig
{
    public string StoreName { get; init; } = null!;
    public string DataPath { get; init; } = null!;
}