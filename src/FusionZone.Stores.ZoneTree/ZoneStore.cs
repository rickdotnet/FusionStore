using System.Diagnostics;
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

    public ValueTask<StoreResult<T>> Get(long id, CancellationToken token)
    {
        if (!zoneTree.TryGet(id, out var json))
            return ValueTask.FromResult(StoreResult.Fail<T>("Item not found"));

        var value = JsonSerializer.Deserialize<T>(json);
        var result = value == null
            ? StoreResult.Fail<T>("Item not found")
            : StoreResult.Success(value);

        return ValueTask.FromResult(result);
    }

    public ValueTask<IEnumerable<StoreResult<T>>> Get(long[] ids, CancellationToken token)
    {
        var results = GetManyInternal(ids);
        return ValueTask.FromResult(results);
    }

    private IEnumerable<StoreResult<T>> GetManyInternal(IEnumerable<long> ids)
    {
        foreach (var id in ids)
        {
            if (!zoneTree.TryGet(id, out var json)) continue;

            var result = JsonSerializer.Deserialize<T>(json);

            if (result != null)
                yield return StoreResult.Success(result);
            else
                yield return StoreResult.Fail<T>("Item not found");
        }
    }

    public async ValueTask<(StoreResult<T> result, long id)> Insert(T data, CancellationToken token)
    {
        var id = data switch { IHaveId hasId => hasId.Id, _ => idGenerator.CreateId() };
        var getResult = await Get(id, token);
        if (getResult)
            return (StoreResult.Fail<T>("Item already exists"), id);
        
        var result = await Save(id, data, token);
        return (result, id);
    }

    public ValueTask<StoreResult<T>> Save(long id, T data, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(data);
        zoneTree.Upsert(id, json);

        return ValueTask.FromResult(StoreResult.Success(data));
    }

    public async ValueTask<StoreResult<T>> Delete(long id, CancellationToken token)
    {
        var itemToDelete = await Get(id, token);
        var result = itemToDelete.Select(x =>
        {
            zoneTree.TryDelete(id, out var _);
            return x;
        });

        return result;
    }
}