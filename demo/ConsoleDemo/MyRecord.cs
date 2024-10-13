using FusionZone.Abstractions;

namespace ConsoleDemo;

public record MyHasIdRecord(long Id, string Message) : IHaveId;
public record MyNonIdRecord(string Message);