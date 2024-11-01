using FusionStore.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;

namespace FusionStore.Stores.Sqlite;

public class SqliteStore<TKey> : DataStore<TKey>
{
    private readonly SqliteCommandManager connection;

    public SqliteStore(SqliteStoreConfig storeConfig, ILogger<SqliteStore<TKey>>? logger = null)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(storeConfig.DataPath, $"{storeConfig.StoreName}.db"),
            Pooling = true
        }.ToString();

        connection = new SqliteCommandManager(connectionString, logger ?? NullLogger<SqliteStore<TKey>>.Instance);
    }
    
    public void Initialize()
    {
        connection.ExecuteCommand(command =>
        {
            command.CommandText = "CREATE TABLE IF NOT EXISTS DataStore (Id TEXT, dataType TEXT, data BLOB)";
            command.ExecuteNonQuery();
        });
        
        // initialized = true; // we're the only user right now, but might need to be more sophisticated in the future
    }

    public override async ValueTask<Result<TData>> Get<TData>(TKey id, CancellationToken token = default)
    {
        var dataType = typeof(TData).Name;
        var result = await connection.ExecuteCommandAsync(async command =>
        {
            command.CommandText = "SELECT data FROM DataStore where Id = @id and dataType = @dataType";
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@dataType", dataType);
            var dbResult = await command.ExecuteScalarAsync(token);
            if (dbResult == null) return default; // returns Result.Failure<TData>("Item not found");

            var bytes = (byte[])dbResult!;
            var data = DefaultSerializer.Deserialize<TData>(bytes);
            return data ?? default; // returns Result.Failure<TData>("Item not found");
        });

        return result!;
    }

    public override ValueTask<(Result<TData> result, TKey id)> Insert<TData>(TData data, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<Result<TData>> Save<TData>(TKey id, TData data, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<Result<TData>> Delete<TData>(TKey id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected override async ValueTask<IEnumerable<TKey>> GetAllIds<TData>(CancellationToken token)
    {
        var dataType = typeof(TData).Name;
        var idResult = await connection.ExecuteCommandAsync(async command =>
        {
            command.CommandText = "SELECT Id FROM DataStore where dataType = @dataType";
            command.Parameters.AddWithValue("@dataType", dataType);
            var reader = await command.ExecuteReaderAsync(token);
            var ids = new List<TKey>();

            while (await reader.ReadAsync(token))
            {
                var bytes = reader.GetFieldValue<byte[]>(0);
                var value = DefaultSerializer.Deserialize<TKey>(bytes);
                ids.Add(value);
            }

            return ids;
        });

        return idResult.ValueOrDefault() ?? [];
    }

    public override async ValueTask<Result<IEnumerable<TData>>> List<TData>(FilterCriteria<TData>? filterCriteria = null, CancellationToken token = default)
    {
        var dataType = typeof(TData).Name;
        var listResult = await connection.ExecuteCommandAsync(async command =>
        {
            command.CommandText = "SELECT data FROM DataStore where dataType = @dataType";
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

            return results.AsEnumerable();
        });

        return listResult;
    }
}