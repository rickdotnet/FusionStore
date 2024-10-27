using ZiggyCreatures.Caching.Fusion;

namespace FusionStore;

public record FusionStoreConfig
{
    public string StoreName { get; init; } = "FusionStore";
    public bool SkipCache { get; set; }
    public FusionCacheEntryOptions? DefaultFusionCacheEntryOptions { get; set; }
}