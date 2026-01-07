using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; protected set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; protected set; } = false;
    
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}