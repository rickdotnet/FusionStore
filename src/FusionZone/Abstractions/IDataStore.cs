namespace FusionZone.Abstractions;

public interface IDataStore<T>
{
    ValueTask<IStoreResult<T>> Get(long id, CancellationToken token);
    ValueTask<IEnumerable<IStoreResult<T>>> Get(long[] ids, CancellationToken token);
    // TODO: determine where GetAll is relevent
    //       ex: does the fusion store need to track Ids?
    //           if it doesn't, then GetALl is a pass-through
    //           this is likely ok, but need to figure out what
    //           the expectation is for GetAll
    //ValueTask<IStoreResult<IEnumerable<T>>> GetAll(bool skipCache, CancellationToken token = default);
    ValueTask<IStoreResult<T>> Insert(T data, CancellationToken token);
    ValueTask<IStoreResult<T>> Save(long id, T data, CancellationToken token);
    ValueTask<IStoreResult<T>> Delete(long id, CancellationToken token);
}