using Microsoft.EntityFrameworkCore;
using ReportingAndPortfolioAnalytics.Domain;
using ReportingAndPortfolioAnalytics.Persistence;

namespace ReportingAndPortfolioAnalytics.Repositories;

public class ReportRepository : IReportRepository
{
	private readonly ReportingDbContext _db;
	public ReportRepository(ReportingDbContext db) => _db = db;

	public async Task<IEnumerable<UWReport>> GetAllAsync(int page, int pageSize) =>
		await _db.UWReports
			.OrderByDescending(r => r.GeneratedDate)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

	public async Task<UWReport?> GetByIdAsync(Guid id) =>
		await _db.UWReports.FindAsync(id);

	public async Task<IEnumerable<UWReport>> GetByScopeAsync(string scope) =>
		await _db.UWReports
			.Where(r => r.Scope.ToLower() == scope.ToLower())
			.OrderByDescending(r => r.GeneratedDate)
			.ToListAsync();

	public async Task<IEnumerable<UWReport>> QueryAsync(string? scope, DateTime? from, DateTime? to)
	{
		var q = _db.UWReports.AsQueryable();

		if (!string.IsNullOrEmpty(scope))
			q = q.Where(r => r.Scope.ToLower() == scope.ToLower());

		if (from.HasValue)
			q = q.Where(r => r.PeriodStart == null || r.PeriodStart >= from.Value);

		if (to.HasValue)
			q = q.Where(r => r.PeriodEnd == null || r.PeriodEnd <= to.Value);

		return await q.OrderByDescending(r => r.PeriodStart).ToListAsync();
	}

	public async Task<UWReport?> GetTodaySnapshotAsync(string scope, string scopeValue)
	{
		var today = DateTime.UtcNow.Date;

		// Check EF local cache first
		var local = _db.UWReports.Local.FirstOrDefault(r =>
			r.Scope == scope &&
			r.ScopeValue == scopeValue &&
			r.PeriodStart.HasValue &&
			r.PeriodStart.Value.Date == today);

		if (local is not null) return local;

		// Fall back to DB
		return await _db.UWReports.FirstOrDefaultAsync(r =>
			r.Scope == scope &&
			r.ScopeValue == scopeValue &&
			r.PeriodStart.HasValue &&
			r.PeriodStart.Value.Date == today);
	}

	public async Task AddAsync(UWReport report) =>
		await _db.UWReports.AddAsync(report);

	public async Task SaveChangesAsync() =>
		await _db.SaveChangesAsync();
}