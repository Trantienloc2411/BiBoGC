using Shared.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Domain.Events
{
    public class CategoryCreatedEvent : DomainEvent
    {
        public Guid CategoryId { get; }
        public string CategoryName { get; } = string.Empty;
        public CategoryCreatedEvent(
            Guid categoryId,
            string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }
    }
}
