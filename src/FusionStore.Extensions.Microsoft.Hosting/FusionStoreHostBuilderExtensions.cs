using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FusionStore.Extensions.Microsoft.Hosting;

public static class FusionStoreHostBuilderExtensions
{
    public static IHostApplicationBuilder AddFusionStore(this IHostApplicationBuilder hostBuilder, Action<IFusionBuilder> configure)
    {
        hostBuilder.Services.AddFusionStore(configure);
        return hostBuilder;
    }

    public static IServiceCollection AddFusionStore(this IServiceCollection services, Action<IFusionBuilder> configure)
    {
        var fusionBuilder = new FusionBuilder(services);
        configure(fusionBuilder);

        return services;
    }
}

