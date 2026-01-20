using Shared.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        = true;

        // Navigation property for parent category
        public Category? ParentCategory { get; set; }
        private readonly List<Category> _subCategories = new();
        public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

        private readonly List<Category> _items = new();
        public IReadOnlyCollection<Category> Items => _items.AsReadOnly();

        private Category()
        {


        }

        public Category(string name, string? description = null, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }
            Name = name.Trim();
            Description = description?.Trim();
            ParentCategoryId = parentCategoryId;
            DisplayOrder = 0;
        }

        public void UpdateInfo(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }
            Name = name.Trim();
            Description = description?.Trim();
        }

        public void SetParentCategory(Guid? parentCategoryId)
        {

            if (parentCategoryId != null && Id == this.Id)
            {
                throw new InvalidOperationException("A category cannot be its own parent.");
            }
            ParentCategoryId = parentCategoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order cannot be negative.");
            }
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
        public void AddSubCategory(Category subCategory)
        {
            if (subCategory == null)
            {
                throw new ArgumentNullException(nameof(subCategory), "Subcategory cannot be null.");
            }
            _subCategories.Add(subCategory);
            UpdatedAt = DateTime.UtcNow;

        }
        public void RemoveSubCategory(Category subCategory)
        {
            if (subCategory == null)
            {
                throw new ArgumentNullException(nameof(subCategory), "Subcategory cannot be null.");
            }
            _subCategories.Remove(subCategory);
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
        
    }
}
