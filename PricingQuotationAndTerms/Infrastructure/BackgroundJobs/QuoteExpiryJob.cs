using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Infrastructure.Data;
using PricingQuotationAndTerms.SharedKernel.Enums;

namespace PricingQuotationAndTerms.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every hour. Finds Draft/Presented quotes past their ValidUntil date
/// and marks them Expired automatically.
/// Registered as IHostedService in DI.
/// </summary>
public class QuoteExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory    _scopeFactory;
    private readonly ILogger<QuoteExpiryJob> _logger;
    private readonly TimeSpan                _interval;

    public QuoteExpiryJob(
        IServiceScopeFactory    scopeFactory,
        ILogger<QuoteExpiryJob> logger,
        IConfiguration          configuration)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;

        var hours = configuration.GetValue<double>("QuoteExpiryJob:IntervalHours", 1);
        _interval = TimeSpan.FromHours(hours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QuoteExpiryJob started. Runs every {Interval}.", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();

            var expiredQuotes = await db.Quotes
                .Where(q => (q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Presented)
                         && q.ValidUntil < DateTime.UtcNow)
                .ToListAsync(ct);

            if (!expiredQuotes.Any())
            {
                _logger.LogDebug("QuoteExpiryJob: no quotes to expire.");
                return;
            }

            foreach (var quote in expiredQuotes)
                quote.MarkExpired();

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "QuoteExpiryJob: expired {Count} quote(s). Ids: {Ids}",
                expiredQuotes.Count,
                string.Join(", ", expiredQuotes.Select(q => q.Id)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "QuoteExpiryJob encountered an error.");
        }
    }
}
