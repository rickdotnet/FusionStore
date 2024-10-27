using System.Runtime.InteropServices;
using Tenray.ZoneTree.Serializers;

namespace FusionStore.Stores.ZoneTree.Internal;

[StructLayout(LayoutKind.Sequential)]
internal struct TypedDeletable
{
    public string TypeName { get; set; }
    public byte[] Data { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public override string ToString()
        => $"Type: {TypeName}, Data: {Data}, IsDeleted: {IsDeleted}, CreatedAt: {CreatedAt}";

    public static TypedDeletable Create<T>(Memory<byte> data, bool isDeleted = false)
        => Create<T>(data.ToArray(), isDeleted);

    public static TypedDeletable Create<T>(byte[] data, bool isDeleted = false)
        => new()
        {
            TypeName = typeof(T).FullName ?? typeof(T).Name,
            Data = data,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = isDeleted
        };
}