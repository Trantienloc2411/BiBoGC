using Sale.Application.Interfaces;

namespace Sale.Infrastructure.Services;

public class OrderNumberGenerator : IOrderNumberGenerator
{
    private static int _dailySequence = 0;
    private static string _lastDate = string.Empty;
    private static readonly object _lock = new();

    public Task<string> GenerateNextAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");

        lock (_lock)
        {
            if (_lastDate != today)
            {
                _lastDate = today;
                _dailySequence = 0;
            }

            _dailySequence++;
            var orderNumber = $"SO-{today}-{_dailySequence:D4}";

            return Task.FromResult(orderNumber);
        }
    }
}