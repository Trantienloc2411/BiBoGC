using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events;

public class CategoryCreatedEvent : DomainEvent
{
    public CategoryCreatedEvent(
        Guid categoryId,
        string categoryName)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
    }

    public Guid CategoryId { get; }
    public string CategoryName { get; } = string.Empty;
}