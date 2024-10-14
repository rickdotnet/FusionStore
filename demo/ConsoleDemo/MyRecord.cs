using FusionZone.Abstractions;

namespace ConsoleDemo;

public record MyRecord(long Id, string Message) : IHaveId;
public record MyNonIdRecord(string Message);