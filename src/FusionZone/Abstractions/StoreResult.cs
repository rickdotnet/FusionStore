namespace FusionZone.Abstractions;

public static class StoreResult
{
    public static StoreResult<T> Success<T>(T value) => new StoreResult<T>.Success(value);
    public static StoreResult<T> Fail<T>(string error) => new StoreResult<T>.Fail(error);
}

public abstract record StoreResult<T>
{
    public sealed record Success(T Value) : StoreResult<T>{}

    public sealed record Fail(string Error) : StoreResult<T>;

    public static implicit operator bool(StoreResult<T> result)
        => result is Success;
}

public static class StoreResultExtensions
{
    public static StoreResult<TResult> Select<T, TResult>(this StoreResult<T> result, Func<T, TResult> transform)
    {
        return result switch
        {
            StoreResult<T>.Success success => StoreResult.Success(transform(success.Value)),
            StoreResult<T>.Fail fail => StoreResult.Fail<TResult>(fail.Error),
            _ => throw new InvalidOperationException("Unknown StoreResult type.")
        };
    }

    public static async ValueTask<StoreResult<TResult>> Select<T, TResult>(this ValueTask<StoreResult<T>> task, Func<T, Task<TResult>> transform)
    {
        var result = await task;

        return result switch
        {
            StoreResult<T>.Success success => StoreResult.Success(await transform(success.Value)),
            StoreResult<T>.Fail fail => StoreResult.Fail<TResult>(fail.Error),
            _ => throw new InvalidOperationException("Unknown StoreResult type.")
        };
    }

    public static async ValueTask<StoreResult<TResult>> Select<T, TResult>(this StoreResult<T> result, Func<T, Task<TResult>> transform)
    {
        return result switch
        {
            StoreResult<T>.Success success => StoreResult.Success(await transform(success.Value)),
            StoreResult<T>.Fail fail => StoreResult.Fail<TResult>(fail.Error),
            _ => throw new InvalidOperationException("Unknown StoreResult type.")
        };
    }

    public static void OnSuccess<T>(this StoreResult<T> result, Action<T> onSuccess)
    {
        if (result is StoreResult<T>.Success success)
            onSuccess(success.Value);
    }

    public static async Task OnSuccess<T>(this StoreResult<T> result, Func<T, Task> onSuccess)
    {
        if (result is StoreResult<T>.Success success)
            await onSuccess(success.Value);
    }

    public static async Task OnSuccess<T>(this ValueTask<StoreResult<T>> task, Func<T, Task> onSuccess)
    {
        var result = await task;
        if (result is StoreResult<T>.Success success)
            await onSuccess(success.Value);
    }
    
    public static async Task OnSuccess<T>(this ValueTask<StoreResult<T>> task, Action<T> onSuccess)
    {
        var result = await task;
        if (result is StoreResult<T>.Success success)
            onSuccess(success.Value);
    }

    public static void OnFailure<T>(this StoreResult<T> result, Action<string> onFailure)
    {
        if (result is StoreResult<T>.Fail fail)
            onFailure(fail.Error);
    }


    public static async Task OnFailure<T>(this StoreResult<T> result, Func<string, Task> onFailure)
    {
        if (result is StoreResult<T>.Fail fail)
            await onFailure(fail.Error);
    }

    public static void Resolve<T>(this StoreResult<T> result, Action<T> onSuccess, Action<string> onFailure)
    {
        switch (result)
        {
            case StoreResult<T>.Success success:
                onSuccess(success.Value);
                break;
            case StoreResult<T>.Fail fail:
                onFailure(fail.Error);
                break;
        }
    }

    public static async Task Resolve<T>(this StoreResult<T> result, Func<T, Task> onSuccess, Func<string, Task> onFailure)
    {
        switch (result)
        {
            case StoreResult<T>.Success success:
                await onSuccess(success.Value);
                break;
            case StoreResult<T>.Fail fail:
                await onFailure(fail.Error);
                break;
        }
    }
}