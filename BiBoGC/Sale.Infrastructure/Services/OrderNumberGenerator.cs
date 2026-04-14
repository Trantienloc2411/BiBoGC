using Microsoft.EntityFrameworkCore;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Infrastructure.Data;

namespace Sale.Infrastructure.Services;

public class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly SaleDbContext _context;

    public OrderNumberGenerator(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

            try
            {
                var sequence = await _context.OrderNumberSequences
                    .FirstOrDefaultAsync(s => s.Date == today, ct);

                if (sequence is null)
                {
                    sequence = new OrderNumberSequence(today);
                    _context.OrderNumberSequences.Add(sequence);
                }

                var newSequence = sequence.Increment();
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return $"SO-{today}-{newSequence:D4}";
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}