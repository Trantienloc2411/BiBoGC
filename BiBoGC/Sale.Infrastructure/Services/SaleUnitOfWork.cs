using Microsoft.EntityFrameworkCore.Storage;
using Sale.Application.Interfaces;
using Sale.Infrastructure.Data;

namespace Sale.Infrastructure.Services;

public class SaleUnitOfWork : ISaleUnitOfWork
{
    private readonly SaleDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public SaleUnitOfWork(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("Chưa có transaction nào được bắt đầu.");

        await _currentTransaction.CommitAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.RollbackAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}