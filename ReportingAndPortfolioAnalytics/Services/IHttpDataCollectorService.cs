namespace ReportingAndPortfolioAnalytics.Services;

public interface IHttpDataCollectorService
{
	/// <summary>
	/// Calls all other microservices via HTTP, collects their data,
	/// and builds a UWReport snapshot for the given scope/period.
	/// Called either on-demand (POST /api/reports/collect) or by the scheduler.
	/// </summary>
	Task CollectAndSnapshotAsync(string scope, string scopeValue,
		DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
}