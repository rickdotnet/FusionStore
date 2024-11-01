using FusionStore.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;

namespace FusionStore.Stores.Sqlite;

// only supporting longs at the moment
public class SqliteStore : DataStore<long>
{
    private readonly SqliteCommandExecutor connection;

    public SqliteStore(SqliteStoreConfig storeConfig, ILogger<SqliteStore>? logger = null)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(storeConfig.DataPath, $"{storeConfig.StoreName}.db"),
            Pooling = true
        }.ToString();

        connection = new SqliteCommandExecutor(connectionString, logger ?? NullLogger<SqliteStore>.Instance);
    }

    public Result<VoidResult> Initialize()
    {
        return Result.Try(() =>
        {
            connection.ExecuteCommand(command =>
            {
                command.CommandText = GetCommandText(Operation.Initialize);
                command.ExecuteNonQuery();
            });

            return VoidResult.Default;
        });
    }

    public async Task<Result<VoidResult>> InitializeAsync()
    {
        var result = await connection.ExecuteCommandAsync<int>(async command =>
        {
            command.CommandText = GetCommandText(Operation.Initialize);
            return await command.ExecuteNonQueryAsync();
        });

        return result.SelectMany(_ => Result.Success(VoidResult.Default));
    }

    public override async ValueTask<Result<TData>> Get<TData>(long id, CancellationToken token = default)
    {
        var dataType = typeof(TData).Name;

        var result = await connection.ExecuteCommandAsync(async command =>
        {
            command.CommandText = GetCommandText(Operation.Get);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@dataType", dataType);

            var dbResult = await command.ExecuteScalarAsync(token);
            if (dbResult == null)
                return Result.Failure<TData>("Item not found"); // Return explicit failure result

            var bytes = (byte[])dbResult;
            var data = DefaultSerializer.Deserialize<TData>(bytes);

            return data != null ? Result.Success(data) : Result.Failure<TData>("Failed to deserialize the result.");
        });

        return result;
    }

    public override async ValueTask<(Result<TData> result, long id)> Insert<TData>(TData data, CancellationToken token = default)
    {
        if (data == null)
            return (Result.Failure<TData>("Cannot save null data"), 0);

        var insertResult = await connection.ExecuteCommandAsync<long>(async command =>
        {
            var dataType = typeof(TData).Name;
            var serializedData = DefaultSerializer.Serialize(data);
            command.CommandText = GetCommandText(Operation.Insert);
            command.Parameters.AddWithValue("@dataType", dataType);
            command.Parameters.AddWithValue("@data", serializedData);

            var newId = (long)(await command.ExecuteScalarAsync(token))!; // this returns the auto-incremented ID
            return newId;
        });

        return insertResult
            ? (data, insertResult.ValueOrDefault())
            : (Result.Failure<TData>("Insert failed"), default);
    }

    public override async ValueTask<Result<TData>> Save<TData>(long id, TData data, CancellationToken token = default)
    {
        if (data == null)
            return Result.Failure<TData>("Cannot save null data");

        var serializedResult = Result.Try(() => DefaultSerializer.Serialize(data));
        var result = await serializedResult.SelectManyAsync(serializedData =>
        {
            return connection.ExecuteCommandAsync<TData>(async command =>
            {
                var dataType = typeof(TData).Name;
                command.CommandText = GetCommandText(Operation.Save);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@dataType", dataType);
                command.Parameters.AddWithValue("@data", serializedData);

                var rowsAffected = await command.ExecuteNonQueryAsync(token);
                if (rowsAffected != 0) return data;

                var (insertResult, _) = await Insert(data, token);
                return insertResult;
            });
        });

        return result;
    }

    public override async ValueTask<Result<TData>> Delete<TData>(long id, CancellationToken token = default)
    {
        var result = await Get<TData>(id, token);
        if (!result) return Result.Failure<TData>("Item not found");

        var delete = await connection.ExecuteCommandAsync<long>(async command =>
        {
            command.CommandText = GetCommandText(Operation.Delete);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@dataType", typeof(TData).Name);

            var rowsAffected = await command.ExecuteNonQueryAsync(token);
            return rowsAffected;
        });

        return delete.ValueOrDefault() > 0 ? result : Result.Failure<TData>("Item not Deleted");
    }

    protected override async ValueTask<IEnumerable<long>> GetAllIds<TData>(CancellationToken token)
    {
        var dataType = typeof(TData).Name;
        var idResult = await connection.ExecuteCommandAsync<List<long>>(async command =>
        {
            command.CommandText = GetCommandText(Operation.GetIds);
            command.Parameters.AddWithValue("@dataType", dataType);
            var reader = await command.ExecuteReaderAsync(token);
            var ids = new List<long>();

            while (await reader.ReadAsync(token))
            {
                var value = reader.GetFieldValue<long>(0);
                ids.Add(value);
            }

            return ids;
        });

        return idResult.ValueOrDefault() ?? [];
    }

    public override async ValueTask<Result<IEnumerable<TData>>> List<TData>(FilterCriteria<TData>? filterCriteria = null, CancellationToken token = default)
    {
        var dataType = typeof(TData).Name;
        var listResult = await connection.ExecuteCommandAsync<IEnumerable<TData>>(async command =>
        {
            command.CommandText = GetCommandText(Operation.List);
            command.Parameters.AddWithValue("@dataType", dataType);

            filterCriteria ??= FilterCriteria.For<TData>();

            if (filterCriteria.SkipValue > 0)
            {
                command.CommandText += " OFFSET @skip"; // skip
                command.Parameters.AddWithValue("@skip", filterCriteria.SkipValue);
            }

            if (filterCriteria.TakeValue < int.MaxValue)
            {
                command.CommandText += " LIMIT @take"; // take
                command.Parameters.AddWithValue("@take", filterCriteria.TakeValue);
            }

            var reader = await command.ExecuteReaderAsync(token);
            var results = new List<TData>();
            var compiledFilter = filterCriteria.Filter.Compile();

            while (await reader.ReadAsync(token))
            {
                var value = reader.GetFieldValue<TData>(0);

                if (value == null) continue;

                if (filterCriteria == null || compiledFilter(value))
                    results.Add(value);
            }

            return Result.Success(results.AsEnumerable());
        });

        return listResult;
    }
    
    private static readonly Dictionary<Operation, string> Commands = new()
    {
        [Operation.Initialize] = "CREATE TABLE IF NOT EXISTS DataStore (Id INTEGER PRIMARY KEY, dataType TEXT, data BLOB)",

        [Operation.Get] = "SELECT data FROM DataStore WHERE Id = @id AND dataType = @dataType",
        
        [Operation.GetIds] = "SELECT Id FROM DataStore WHERE dataType = @dataType",

        [Operation.Insert] =
            "INSERT INTO DataStore (dataType, data) VALUES (@dataType, @data); SELECT last_insert_rowid();",
        
        [Operation.Save] = 
            "UPDATE DataStore SET data = @data WHERE Id = @id AND dataType = @dataType",
        
        [Operation.Delete] = 
            "DELETE FROM DataStore WHERE Id = @id AND dataType = @dataType",
        
        [Operation.List] =
            "SELECT data FROM DataStore where dataType = @dataType"
    };

    private static string GetCommandText(Operation operation)
    {
        if (!Commands.TryGetValue(operation, out var commandText))
            throw new InvalidOperationException($"No command text found for operation {operation}.");

        return commandText;
    }
    
    enum Operation
    {
        Initialize,
        Get,
        GetIds,
        Insert,
        Save,
        Delete,
        List
    }
}