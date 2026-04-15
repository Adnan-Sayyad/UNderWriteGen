using ReportingAndPortfolioAnalytics.Domain;

namespace ReportingAndPortfolioAnalytics.Repositories;

public interface IReportRepository
{
	Task<IEnumerable<UWReport>> GetAllAsync(int page, int pageSize);
	Task<UWReport?> GetByIdAsync(Guid id);
	Task<IEnumerable<UWReport>> GetByScopeAsync(string scope);
	Task<IEnumerable<UWReport>> QueryAsync(string? scope, DateTime? from, DateTime? to);
	Task<UWReport?> GetTodaySnapshotAsync(string scope, string scopeValue);
	Task AddAsync(UWReport report);
	Task SaveChangesAsync();
}