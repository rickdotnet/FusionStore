namespace FusionStore.Stores.ZoneTree;

public record ZoneStoreConfig
{
    public string StoreName { get; set; } = "ZoneStore";
    public required string DataPath { get; init; }
}