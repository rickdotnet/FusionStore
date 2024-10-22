using RickDotNet.Base;

namespace FusionZone.Abstractions;

public interface IDataStore : IDataStore<long>; 
public interface IDataStore<TKey> 
{
    ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token);
    ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token);
    //ValueTask<Result<T>> Insert(T data, CancellationToken token);
    ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token);
    ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token);
}