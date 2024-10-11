namespace FusionZone.Abstractions;

public interface IDataStore<T>
{
    ValueTask<IStoreResult<T>> Get(long id, CancellationToken token);
    ValueTask<IStoreResult<T>> Insert(T data, CancellationToken token);
    ValueTask<IStoreResult<T>> Save(long id, T data, CancellationToken token);
    ValueTask<IStoreResult<T>> Delete(long id, CancellationToken token);
}