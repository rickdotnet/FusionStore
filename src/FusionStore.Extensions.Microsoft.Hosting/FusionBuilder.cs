using Microsoft.Extensions.DependencyInjection;

namespace FusionStore.Extensions.Microsoft.Hosting;

public interface IFusionBuilder
{
    IFusionBuilder WithKey<TKey>(Action<IFusionStoreBuilder<TKey>> configure);
}
public class FusionBuilder : IFusionBuilder
{
    private readonly IServiceCollection hostBuilderServices;
    private readonly HashSet<string> storeNames = new();

    public FusionBuilder(IServiceCollection hostBuilderServices)
    {
        this.hostBuilderServices = hostBuilderServices;
    }

    private static string GetStoreKeyName(string storeName) => $"Fusion-{storeName}";

    public IFusionBuilder WithKey<TKey>(Action<IFusionStoreBuilder<TKey>> configure)
    {
        var storeBuilder = new FusionStoreBuilder<TKey>();
        configure(storeBuilder);

        var storeName = GetStoreKeyName(storeBuilder.Config.StoreName);
        if (storeNames.Contains(storeName))
        {
            // TODO: log warning?
        }

        var fusionStore = storeBuilder.Build();
        hostBuilderServices.AddKeyedSingleton(storeName, fusionStore);
        hostBuilderServices.AddSingleton(fusionStore);

        storeNames.Add(storeName);

        return this;
    }
}