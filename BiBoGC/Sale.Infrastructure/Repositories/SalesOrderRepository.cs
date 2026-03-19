using Microsoft.EntityFrameworkCore;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Domain.Enum;
using Sale.Infrastructure.Data;

namespace Sale.Infrastructure.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly SaleDbContext _context;

    public SalesOrderRepository(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SalesOrders
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<SalesOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SalesOrders
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _context.SalesOrders
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, ct);
    }

    public async Task<(IEnumerable<SalesOrder> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        OrderStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = _context.SalesOrders
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(x => x.OrderDate >= DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc));

        if (dateTo.HasValue)
            query = query.Where(x => x.OrderDate <= DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(x =>
                x.OrderNumber.ToLower().Contains(searchLower) ||
                (x.CustomerName != null && x.CustomerName.ToLower().Contains(searchLower)) ||
                (x.CustomerPhone != null && x.CustomerPhone.Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken ct = default)
    {
        await _context.SalesOrders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task UpdateAsync(SalesOrder order, CancellationToken ct = default)
    {
        // Disable auto-detect so that Entry() calls below do NOT trigger DetectChanges.
        // Without this, DetectChanges would find the new item (which has a non-default GUID
        // from BaseEntity) in the navigation collection and track it as Unchanged, causing
        // SaveChanges to emit UPDATE instead of INSERT → DbUpdateConcurrencyException (0 rows).
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            var entry = _context.Entry(order);
            if (entry.State == EntityState.Detached)
                throw new InvalidOperationException(
                    $"SalesOrder {order.Id} must be loaded via GetByIdWithItemsAsync before calling UpdateAsync " +
                    "so that EF Core tracks it. Detached updates are not supported.");

            foreach (var item in order.Items)
                if (_context.Entry(item).State == EntityState.Detached)
                    _context.SalesOrderItems.Add(item); // new item → INSERT
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _context.SalesOrders
            .AnyAsync(x => x.OrderNumber == orderNumber, ct);
    }
}