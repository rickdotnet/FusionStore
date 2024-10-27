using ZiggyCreatures.Caching.Fusion;

namespace FusionStore;

public record FusionStoreConfig
{
    public required string StoreName { get; init; }
    public bool SkipCache { get; set; }
    public FusionCacheEntryOptions? DefaultFusionCacheEntryOptions { get; set; }
}