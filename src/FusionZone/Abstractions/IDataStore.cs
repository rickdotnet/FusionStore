namespace FusionZone.Abstractions;

public interface IDataStore : IDataStore<long>; 
public interface IDataStore<TKey> 
{
    ValueTask<StoreResult<TData>> Get<TData>(TKey id, CancellationToken token);
    ValueTask<(StoreResult<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token);
    //ValueTask<StoreResult<T>> Insert(T data, CancellationToken token);
    ValueTask<StoreResult<TData>> Save<TData>(TKey id, TData data, CancellationToken token);
    ValueTask<StoreResult<TData>> Delete<TData>(TKey id, CancellationToken token);
}