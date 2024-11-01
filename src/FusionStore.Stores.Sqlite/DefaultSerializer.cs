using System.Text.Json;

namespace FusionStore.Stores.Sqlite;

internal class DefaultSerializer
{
    public static TData Deserialize<TData>(Memory<byte> bytes)
        => JsonSerializer.Deserialize<TData>(bytes.Span) ?? default!;

    public static Memory<byte> Serialize<TData>(in TData entry)
        => JsonSerializer.SerializeToUtf8Bytes(entry);
    
    //public static DefaultSerializer<TData> For<TData>() => new();
}