namespace Shared.Domain.Common;

public abstract class DomainEvent
{
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
}