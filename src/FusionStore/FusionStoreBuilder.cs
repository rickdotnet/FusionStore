using FusionStore.Abstractions;

namespace FusionStore;

public interface IFusionStoreBuilder
{
    FusionStoreConfig Config { get; }
}

public interface IFusionStoreBuilder<TKey> : IFusionStoreBuilder
{
    IFusionStoreBuilder<TKey> WithConfig(FusionStoreConfig config);
    IFusionStoreBuilder<TKey> WithInnerStore(IDataStore<TKey> store);
    IFusionStore<TKey> Build();
}

public class FusionStoreBuilder<TKey> : IFusionStoreBuilder<TKey>
{
    public FusionStoreConfig Config { get; private set; } = new();
    private IDataStore<TKey>? innerStore;

    public IFusionStoreBuilder<TKey> WithConfig(FusionStoreConfig storeConfig)
    {
        Config = storeConfig;
        return this;
    }

    public IFusionStoreBuilder<TKey> WithInnerStore(IDataStore<TKey> store)
    {
        innerStore = store;
        return this;
    }

    public IFusionStore<TKey> Build()
    {
        if (innerStore == null)
            throw new InvalidOperationException("Inner store must be set before building");

        return new FusionStore<TKey>(innerStore, Config);
    }
}