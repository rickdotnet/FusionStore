using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RickDotNet.Base;

namespace FusionStore.Stores.Sqlite;

public class SqliteCommandManager
{
    private readonly string connectionString;
    private readonly ILogger logger;

    public SqliteCommandManager(string connectionString, ILogger logger)
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

    public async Task<Result<T>> ExecuteCommandAsync<T>(Func<SqliteCommand, Task<T>> execute)
    {
        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();

            var result = await execute(command);
            return result ?? Result.Failure<T>("Item not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute SQLite command");
            return ex;
        }
    }
}