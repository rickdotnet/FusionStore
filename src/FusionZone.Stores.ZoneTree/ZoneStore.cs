using System.Text.Json;
using FusionZone.Abstractions;
using IdGen;
using Tenray.ZoneTree;

namespace FusionZone.Stores.ZoneTree;

public class ZoneStore<TKey> : IDataStore<TKey>
{
    private readonly IZoneTree<TKey, string> zoneTree;
    private readonly IIdGenerator<TKey> idGenerator;

    public ZoneStore(IZoneTree<TKey, string> zoneTree, IIdGenerator<TKey> idGenerator)
    {
        this.zoneTree = zoneTree;
        this.idGenerator = idGenerator;
    }

    public ValueTask<StoreResult<TData>> Get<TData>(TKey id, CancellationToken token)
    {
        if (!zoneTree.TryGet(id, out var json))
            return ValueTask.FromResult(StoreResult.Fail<TData>(new Exception("Item not found")));

        var value = JsonSerializer.Deserialize<TData>(json);
        var result = value == null
            ? StoreResult.Fail<TData>(new Exception("Item not found"))
            : StoreResult.Success(value);

        return ValueTask.FromResult(result);
    }

    public ValueTask<IEnumerable<StoreResult<TData>>> Get<TData>(TKey[] ids, CancellationToken token)
    {
        var results = GetManyInternal<TData>(ids);
        return ValueTask.FromResult(results);
    }

    private IEnumerable<StoreResult<TData>> GetManyInternal<TData>(IEnumerable<TKey> ids)
    {
        foreach (var id in ids)
        {
            if (!zoneTree.TryGet(id, out var json)) continue;

            var result = JsonSerializer.Deserialize<TData>(json);

            if (result != null)
                yield return StoreResult.Success(result);
            else
                yield return StoreResult.Fail<TData>(new Exception("Item not found"));
        }
    }

    public async ValueTask<(StoreResult<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token)
    {
        var id = data switch { IHaveId<TKey> hasId => hasId.Id, _ => idGenerator.CreateId() };
        var getResult = await Get<TData>(id, token);
        if (getResult)
            return (StoreResult.Fail<TData>(new Exception("Item already exists")), id);

        var result = await Save<TData>(id, data, token);
        return (result, id);
    }

    public ValueTask<StoreResult<TData>> Save<TData>(TKey id, TData data, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(data);
        zoneTree.Upsert(id, json);

        return ValueTask.FromResult(StoreResult.Success(data));
    }

    public async ValueTask<StoreResult<TData>> Delete<TData>(TKey id, CancellationToken token)
    {
        var itemToDelete = await Get<TData>(id, token);
        var result = itemToDelete.Select(x =>
        {
            zoneTree.TryDelete(id, out var _);
            return x;
        });

        return result;
    }
}