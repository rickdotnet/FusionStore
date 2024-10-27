using System.Text.Json;
using Tenray.ZoneTree.Serializers;

namespace FusionStore.Stores.ZoneTree.Internal;

public interface IZoneSerializer<TData> : ISerializer<TData>;

// used by ZoneTree if none registered with ZoneTreeRegistry
internal sealed class DefaultZoneSerializer<TData> : IZoneSerializer<TData>
{
    public TData Deserialize(Memory<byte> bytes) 
        => DefaultZoneSerializer.Deserialize<TData>(bytes) ?? default!;

    public Memory<byte> Serialize(in TData entry) 
        => DefaultZoneSerializer.Serialize(entry);
}

// used by ZoneStore if none registered with ZoneTreeRegistry
internal class DefaultZoneSerializer
{
    public static TData Deserialize<TData>(Memory<byte> bytes)
        => JsonSerializer.Deserialize<TData>(bytes.Span) ?? default!;

    public static Memory<byte> Serialize<TData>(in TData entry)
        => JsonSerializer.SerializeToUtf8Bytes(entry);
    
    public static DefaultZoneSerializer<TData> For<TData>() => new();
}