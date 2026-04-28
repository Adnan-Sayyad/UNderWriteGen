using ReportingAndPortfolioAnalytics.DTOs;

namespace ReportingAndPortfolioAnalytics.Services;

public interface IReportService
{
	Task<IEnumerable<ReportSummaryDto>> GetAllAsync(int page, int pageSize);
	Task<ReportDetailDto?> GetByIdAsync(Guid id);
	Task<IEnumerable<ReportSummaryDto>> GetByScopeAsync(string scope);
	Task<ReportDetailDto> GenerateAsync(GenerateReportRequestDto request, string generatedBy);
	Task<IEnumerable<HitRatioDto>> GetHitRatioAsync(string? scope, DateTime? from, DateTime? to);
	Task<IEnumerable<TatDto>> GetTatAsync(string? scope, DateTime? from, DateTime? to);
	Task<IEnumerable<ReferralRateDto>> GetReferralRateAsync(string? scope, DateTime? from, DateTime? to);
	Task<IEnumerable<PremiumDistributionDto>> GetPremiumDistributionAsync(string? scope, DateTime? from, DateTime? to);
	Task<IEnumerable<RiskMixDto>> GetRiskMixAsync(string? scope, DateTime? from, DateTime? to);
	Task<IEnumerable<UWProductivityDto>> GetUWProductivityAsync(string? scope, DateTime? from, DateTime? to);
}