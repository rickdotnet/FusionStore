using ZiggyCreatures.Caching.Fusion;

namespace FusionZone;

public record FusionStoreConfig
{
    public required string StoreName { get; init; }
    public bool SkipCache { get; set; }
    public FusionCacheEntryOptions? DefaultFusionCacheEntryOptions { get; set; }
}