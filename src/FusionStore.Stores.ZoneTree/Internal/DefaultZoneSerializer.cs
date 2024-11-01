using Tenray.ZoneTree.Serializers;

namespace FusionStore.Stores.ZoneTree.Internal;

public interface IZoneSerializer<TData> : ISerializer<TData>;

// used by ZoneTree if none registered with ZoneTreeRegistry
internal sealed class DefaultZoneSerializer<TData> : IZoneSerializer<TData>
{
    public TData Deserialize(Memory<byte> bytes) 
        => DefaultSerializer.Deserialize<TData>(bytes) ?? default!;

    public Memory<byte> Serialize(in TData entry) 
        => DefaultSerializer.Serialize(entry);
}

internal class DefaultZoneSerializer
{
    public static DefaultZoneSerializer<TData> For<TData>() => new();
}