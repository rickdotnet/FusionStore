using RickDotNet.Base;
using RickDotNet.Extensions.Base;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.PresetTypes;
using Tenray.ZoneTree.Serializers;

namespace FusionZone.Stores.ZoneTree;

public class ZoneTreeRegistry
{
    public static readonly ZoneTreeRegistry Default = new();

    private readonly Dictionary<Type, object> serializerMap = new();
    private readonly Dictionary<Type, object> comparerMap = new();

    public void RegisterSerializer<T>(ISerializer<T> serializer)
    {
        serializerMap[typeof(T)] = serializer;
    }

    public void RegisterComparer<T>(IRefComparer<T> comparer)
    {
        comparerMap[typeof(T)] = comparer;
    }

    public ISerializer<T>? GetSerializer<T>()
    {
        if (serializerMap.TryGetValue(typeof(T), out var serializer))
            return (ISerializer<T>)serializer;

        var result = Result.Try(ComponentsForKnownTypes.GetSerializer<T>);
        return result.ValueOrDefault();
    }

    public IRefComparer<T>? GetComparer<T>()
    {
        if (comparerMap.TryGetValue(typeof(T), out var comparer))
            return (IRefComparer<T>)comparer;
        
        var result = Result.Try(ComponentsForKnownTypes.GetComparer<T>);
        return result.ValueOrDefault();
    }
}