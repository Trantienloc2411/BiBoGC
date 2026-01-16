using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly InventoryDbContext _context;
        
        public SupplierRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return supplier;
        }

        public async Task DeleteAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            supplier.SoftDelete();
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetAllAsync(int pageSize = 10, int pageNumber = 1, string? searchTerm = null, bool? isActive = null, string? sortBy = "Name", bool sortDescending = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.Trim();

                query = query.Where
                    (s => EF.Functions.ILike(s.Name, $"%{search}%") ||
                    (s.ContactPerson != null && EF.Functions.ILike(s.ContactPerson, $"%{search}%")) ||
                    (s.PhoneNumber != null && EF.Functions.ILike(s.PhoneNumber, $"%{search}%")) ||
                    (s.Address != null && EF.Functions.ILike(s.Address, $"%{search}%")));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),
                "createdat" => sortDescending ? query.OrderByDescending(s => s.CreatedAt) :
                query.OrderBy(s => s.CreatedAt),
                _ => query.OrderBy(s => s.Name)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()  // Tối ưu: không track changes nếu chỉ đọc
                .ToListAsync(cancellationToken);

            return (items, totalCount);

        }

        public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .Include(s => s.Transactions.OrderByDescending(t => t.CreatedAt).Take(5))
                .ThenInclude(s => s.ProductBatch)
                .ThenInclude(pb => pb.Product)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeSupplierId = null, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.Trim().ToLower();
            var query = _context.Suppliers.AsQueryable();

            var result = await query.AnyAsync(s => s.Name.ToLower() == normalizedName && (excludeSupplierId == null || s.Id != excludeSupplierId.Value), cancellationToken);

            return result;

        }



        public async Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.Where(s => s.Id == supplier.Id).ForEachAsync(s =>
            {
                s.UpdateInfo(supplier.Name, supplier.ContactPerson, supplier.PhoneNumber, supplier.Address);
                if (supplier.IsActive)
                {
                    s.Activate();
                }
                else
                {
                    s.Deactivate();
                }
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
