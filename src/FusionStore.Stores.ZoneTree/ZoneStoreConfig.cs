﻿namespace FusionStore.Stores.ZoneTree;

public record ZoneStoreConfig
{
    public required string StoreName { get; init; }
    public required string DataPath { get; init; }
}