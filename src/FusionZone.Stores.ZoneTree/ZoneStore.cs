using FusionZone.Abstractions;
using FusionZone.Stores.ZoneTree.Internal;
using IdGen;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;
using Tenray.ZoneTree;

namespace FusionZone.Stores.ZoneTree;

public class ZoneStore<TKey> : DataStore<TKey>
{
    private readonly IZoneTree<TKey, TypedDeletable> zoneTree;
    private readonly ZoneIndex<TKey> zoneIndex;
    private readonly IIdGenerator<TKey> idGenerator;

    public ZoneStore(ZoneStoreConfig storeConfig, IIdGenerator<TKey> idGenerator)
    {
        this.idGenerator = idGenerator;

        zoneTree = FuzionZoneTreeFactory.Create<TKey>(storeConfig);
        zoneIndex = ZoneIndex.Create<TKey>(storeConfig);
    }

    public override ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return ValueTask.FromResult(Result.Failure<TData>("Operation cancelled"));

        var entry = GetTypedDeletable(id);
        var result = GetValueFromDeletable<TData>(entry);

        return ValueTask.FromResult(result);
    }

    private Result<TypedDeletable> GetTypedDeletable(TKey key)
    {
        return zoneTree.TryGet(key, out var entry)
            ? entry
            : Result.Failure<TypedDeletable>($"Item not found: {key?.ToString()}");
    }

    private static Result<TData> GetValueFromDeletable<TData>(Result<TypedDeletable> deletable)
    {
        var serializer = ZoneTreeRegistry.Default.GetSerializer<TData>();
        var result = deletable.SelectMany(
            entry =>
            {
                var value = serializer != null
                    ? serializer.Deserialize(entry.Data)
                    : DefaultZoneSerializer.Deserialize<TData>(entry.Data);

                return value == null
                    ? Result.Failure<TData>("Unable to deserialize TypedDeletable")
                    : Result.Success(value);
            });

        return result;
    }


    public override async ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token = default)
    {
        var id = data switch { IHaveId<TKey> hasId => hasId.Id, _ => idGenerator.CreateId() };
        var getResult = await Get<TData>(id, token);
        if (getResult)
            return (Result.Failure<TData>(new Exception("Item already exists")), id);

        var result = await Save(id, data, token);
        return (result, id);
    }

    public override ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token = default)
    {
        var serializer = ZoneTreeRegistry.Default.GetSerializer<TData>();
        var serialized = serializer?.Serialize(data) ?? DefaultZoneSerializer.Serialize(data);

        var entry = TypedDeletable.Create<TData>(serialized);
        var result = Result.Try(() =>
        {
            zoneTree.Upsert(id, entry);
            zoneIndex.Add<TData>(id);
            return data;
        });

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token = default)
    {
        var result = await Get<TData>(id, token);
        result.Select(item =>
        {
            var success = zoneTree.TryDelete(id, out _);
            zoneIndex.Remove<TData>(id);

            return success
                ? Result.Success(item)
                : Result.Failure<TData>($"Item not found: {id?.ToString()}");
        });

        return result;
    }

    protected override ValueTask<IEnumerable<TKey>> GetAllIds<TData>(CancellationToken token)
    {
        // determine strategy for this
        return ValueTask.FromResult(zoneIndex.GetAllIds<TData>());
    }
}