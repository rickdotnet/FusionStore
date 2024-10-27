namespace FusionStore.Abstractions;

public interface IHaveId: IHaveId<long> { }
public interface IHaveId<out TKey>
{
    public TKey Id { get; }
}