using Tenray.ZoneTree.Comparers;

namespace FusionZone.Stores.ZoneTree;

public record ZoneStoreConfig
{
    public required string StoreName { get; init; }
    public required string DataPath { get; init; }
}