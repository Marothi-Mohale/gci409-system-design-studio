namespace Gci409.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public interface IAggregateRoot
{
}
