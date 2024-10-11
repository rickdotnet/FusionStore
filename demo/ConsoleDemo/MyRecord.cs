namespace ConsoleDemo;

public record MyRecord
{
    public int Id { get; set; }
    public required string Message { get; set; }

    public override string ToString()
    {
        return Message;
    }
}