using Microsoft.EntityFrameworkCore;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Infrastructure.Data;

namespace Sale.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly SaleDbContext _context;

    public InvoiceNumberGenerator(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(CancellationToken ct = default)
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

            try
            {
                var sequence = await _context.InvoiceNumberSequences
                    .FirstOrDefaultAsync(s => s.YearMonth == yearMonth, ct);

                if (sequence is null)
                {
                    sequence = new InvoiceNumberSequence(yearMonth);
                    _context.InvoiceNumberSequences.Add(sequence);
                }

                var newSequence = sequence.Increment();
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return $"INV-{yearMonth}-{newSequence:D4}";
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}