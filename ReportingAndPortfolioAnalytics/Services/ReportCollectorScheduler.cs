namespace ReportingAndPortfolioAnalytics.Services;

/// <summary>
/// Runs HttpDataCollectorService on a configurable interval.
/// Collects data for all product lines automatically in the background.
/// </summary>
public class ReportCollectorScheduler : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<ReportCollectorScheduler> _logger;
	private readonly TimeSpan _interval;

	// Product lines to collect — expand this list as new lines go live
	private static readonly string[] ProductLines =
		{ "Motor", "Health", "Life", "Commercial" };

	public ReportCollectorScheduler(
		IServiceScopeFactory scopeFactory,
		ILogger<ReportCollectorScheduler> logger,
		IConfiguration config)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;

		// Default: collect every 4 hours. Override in appsettings.json
		var hours = config.GetValue<int>("Collector:IntervalHours", 4);
		_interval = TimeSpan.FromHours(hours);
	}

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		_logger.LogInformation("Report collector scheduler started. Interval: {Hours}h",
			_interval.TotalHours);

		// Small delay on startup so other services have time to start
		await Task.Delay(TimeSpan.FromSeconds(15), ct);

		while (!ct.IsCancellationRequested)
		{
			await RunCollectionAsync(ct);
			await Task.Delay(_interval, ct);
		}
	}

	private async Task RunCollectionAsync(CancellationToken ct)
	{
		var today = DateTime.UtcNow.Date;

		foreach (var productLine in ProductLines)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var collector = scope.ServiceProvider
					.GetRequiredService<IHttpDataCollectorService>();

				await collector.CollectAndSnapshotAsync(
					scope: "Product",
					scopeValue: productLine,
					periodStart: today,
					periodEnd: today.AddDays(1).AddTicks(-1),
					ct: ct);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				_logger.LogError(ex, "Collection failed for ProductLine={Line}", productLine);
			}
		}
	}
}