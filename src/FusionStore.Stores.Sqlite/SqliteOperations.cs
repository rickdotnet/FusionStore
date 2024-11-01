namespace FusionStore.Stores.Sqlite;

public enum Operation
{
    Initialize,
    Get,
    Insert,
    Save,
    Remove,
    List
}

public static class SqliteOperations
{
    private static readonly Dictionary<Operation, string> commands = new()
    {
        [Operation.Initialize] =
            """
           
            """,

        [Operation.Get] =
            "SELECT data FROM DataStore where Id = @id and dataType = @dataType",

        [Operation.Insert] =
            "",
        
        [Operation.Save] =
            "",
        
        [Operation.Remove] = 
            "",
        
        [Operation.List] =
            "SELECT data FROM DataStore where dataType = @dataType"
    };

    public static string GetCommandText(Operation operation)
    {
        if (!commands.TryGetValue(operation, out var commandText))
            throw new InvalidOperationException($"No command text found for operation {operation}.");

        return commandText;
    }
}