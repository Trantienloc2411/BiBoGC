using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplerRepository
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

        public async Task<IEnumerable<Supplier>> GetAllAsync(int pageSize = 10, int pageNumber = 1, CancellationToken cancellationToken = default)
        {
            var suppliers = await _context.Suppliers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);    
            return suppliers;
        }

        public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.Include(c => c.Transactions).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);    
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeSupplierId = null, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.Trim().ToLower();
            var query = _context.Suppliers.AsQueryable();

            var result = await query.AnyAsync(s =>s.Name.ToLower() == normalizedName && (excludeSupplierId == null || s.Id != excludeSupplierId.Value), cancellationToken);
            
            return result;

        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            if(searchTerm is null or "")
            {
                return await GetAllAsync(cancellationToken: cancellationToken);
            }
            else
            {
                searchTerm = searchTerm.Trim().ToLower();
                var query = _context.Suppliers.AsQueryable();
                
                var result = query.Where(s => s.Name.ToLower().Contains(searchTerm) || 
                                              (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(searchTerm)) ||
                                              (s.PhoneNumber != null && s.PhoneNumber.ToLower().Contains(searchTerm)) ||
                                              (s.Address != null && s.Address.ToLower().Contains(searchTerm))
                                        );

                return await result.ToListAsync(cancellationToken);
            }
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
