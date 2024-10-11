namespace FusionZone.Abstractions;

public interface IStoreResult<out T>
{
    bool Succeeded { get; }
    long Id { get; }
    T? Value { get; }
    string? Error { get; }
}

public sealed class StoreResult<T> : IStoreResult<T>
{
    public bool Succeeded { get; }
    public long Id { get; }
    public T? Value { get; }
    public string? Error { get; }

    private StoreResult(bool succeeded, long id, T? value, string? error)
    {
        Id = id;
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public static IStoreResult<T> Success(long id, T? value) => new StoreResult<T>(true, id, value, null);
    public static IStoreResult<T> Fail(long id, string error) => new StoreResult<T>(false, id, default, error);
}