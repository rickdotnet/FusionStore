using System.Text.Json;
using FusionZone.Abstractions;
using IdGen;
using Tenray.ZoneTree;

namespace FusionZone.Stores.ZoneTree;

public class ZoneStore<T> : IDataStore<T>
{
    private readonly IZoneTree<long, string> zoneTree;
    private readonly IIdGenerator<long> idGenerator;

    public ZoneStore(IZoneTree<long, string> zoneTree, IIdGenerator<long> idGenerator)
    {
        this.zoneTree = zoneTree;
        this.idGenerator = idGenerator;
    }

    public ValueTask<IStoreResult<T>> Get(long id, CancellationToken token)
    {
        if (!zoneTree.TryGet(id, out var json))
            return ValueTask.FromResult(StoreResult<T>.Fail(id, "Item not found"));

        var result = JsonSerializer.Deserialize<T>(json);
        return ValueTask.FromResult(StoreResult<T>.Success(id, result));
    }

    public ValueTask<IStoreResult<T>> Insert(T data, CancellationToken token)
    {
        var id = idGenerator.CreateId();
        return Save(id, data, token);
    }

    public ValueTask<IStoreResult<T>> Save(long id, T data, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(data);
        zoneTree.Upsert(id, json);

        return ValueTask.FromResult(StoreResult<T>.Success(id, data));
    }

    public async ValueTask<IStoreResult<T>> Delete(long id, CancellationToken token)
    {
        var itemToDelete = await Get(id, token);
        if (itemToDelete.Value == null)
            return StoreResult<T>.Fail(id, "Item not found.");

        return zoneTree.TryDelete(id, out var _) 
            ? StoreResult<T>.Success(id, itemToDelete.Value) 
            : StoreResult<T>.Fail(id, "Item not found.");
    }
}