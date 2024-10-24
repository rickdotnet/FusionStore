namespace FusionZone.Stores.ZoneTree;

// not sure I need this, but don't want to throw it away yet
public abstract class IndexedItem
{
    public static IndexedItem<TKey, TData> Create<TKey, TData>(TKey key, TData data)
        => new()
        {
            Key = key,
            Data = data
        };
}
public record IndexedItem<TKey, TData>
{
    public TKey Key { get; init; }
    public TData Data { get; init; }
}