namespace FusionZone.Abstractions;

public interface IDataStore<TData> : IDataStore<long, TData>; 
public interface IDataStore<TKey,TData> 
{
    ValueTask<StoreResult<TData>> Get(TKey id, CancellationToken token);
    ValueTask<(StoreResult<TData> result, TKey id)> Insert(TData data, CancellationToken token);
    //ValueTask<StoreResult<T>> Insert(T data, CancellationToken token);
    ValueTask<StoreResult<TData>> Save(TKey id, TData data, CancellationToken token);
    ValueTask<StoreResult<TData>> Delete(TKey id, CancellationToken token);
}