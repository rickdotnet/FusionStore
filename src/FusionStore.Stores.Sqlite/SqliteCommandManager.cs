using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RickDotNet.Base;

namespace FusionStore.Stores.Sqlite;

public class SqliteCommandExecutor
{
    private readonly string connectionString;
    private readonly ILogger logger;

    public SqliteCommandExecutor(string connectionString, ILogger logger)
    {
        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ExecuteCommand(Action<SqliteCommand> execute)
    {
        using var connection = new SqliteConnection(connectionString);
        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            execute(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute SQLite command");
            throw;
        }
    }

    public Task<Result<T>> ExecuteCommandAsync<T>(Func<SqliteCommand, Task<Result<T>>> execute)
    {
        return Result.TryAsync(async () =>
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();

            return await execute(command);  
        });
    }
    
    public Task<Result<T>> ExecuteTransactionAsync<T>(Func<DbTransaction, Task<Result<T>>> execute)
    {
        return Result.TryAsync(async () =>
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var result = await execute(transaction);  // execute within transaction
                if (result)
                    await transaction.CommitAsync();  // commit if all succeeds
                else
                    await transaction.RollbackAsync();  // rollback if failure

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();  // ensure rollback on exception
                logger.LogError(ex, "Failed to execute transaction");
                throw;
            }
        });
    }
}