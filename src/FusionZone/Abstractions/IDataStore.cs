namespace FusionZone.Abstractions;

public interface IDataStore<T> 
{
    ValueTask<StoreResult<T>> Get(long id, CancellationToken token);
    ValueTask<(StoreResult<T> result, long id)> Insert(T data, CancellationToken token);
    //ValueTask<StoreResult<T>> Insert(T data, CancellationToken token);
    ValueTask<StoreResult<T>> Save(long id, T data, CancellationToken token);
    ValueTask<StoreResult<T>> Delete(long id, CancellationToken token);
}