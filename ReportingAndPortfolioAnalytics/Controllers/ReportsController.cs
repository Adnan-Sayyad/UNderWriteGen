using Microsoft.AspNetCore.Mvc;
using ReportingAndPortfolioAnalytics.DTOs;
using ReportingAndPortfolioAnalytics.Services;
using System.Security.Claims;

namespace ReportingAndPortfolioAnalytics.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
	private readonly IReportService _svc;
	private readonly IHttpDataCollectorService _collector;

	// Product lines — mirrors ReportCollectorScheduler
	private static readonly string[] ProductLines = { "Life", "Health", "PnC", "Commercial" };

	public ReportsController(IReportService svc, IHttpDataCollectorService collector)
	{
		_svc       = svc;
		_collector = collector;
	}

	// GET /api/reports?page=1&pageSize=20
	[HttpGet]
	public async Task<IActionResult> GetAll(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20)
	{
		if (page < 1 || pageSize < 1 || pageSize > 100)
			return BadRequest("page ≥ 1 and pageSize 1–100.");
		return Ok(await _svc.GetAllAsync(page, pageSize));
	}

	// GET /api/reports/{id}
	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var r = await _svc.GetByIdAsync(id);
		return r is null ? NotFound($"Report {id} not found.") : Ok(r);
	}

	// GET /api/reports/scope/Product
	[HttpGet("scope/{scope}")]
	public async Task<IActionResult> GetByScope(string scope)
	{
		var valid = new[] { "Product", "Region", "Agent", "Period" };
		if (!valid.Contains(scope, StringComparer.OrdinalIgnoreCase))
			return BadRequest($"Scope must be one of: {string.Join(", ", valid)}");
		return Ok(await _svc.GetByScopeAsync(scope));
	}

	// POST /api/reports/generate
	[HttpPost("generate")]
	public async Task<IActionResult> Generate([FromBody] GenerateReportRequestDto request)
	{
		if (string.IsNullOrWhiteSpace(request.Scope) || string.IsNullOrWhiteSpace(request.ScopeValue))
			return BadRequest("Scope and ScopeValue are required.");
		if (request.PeriodStart >= request.PeriodEnd)
			return BadRequest("PeriodStart must be before PeriodEnd.");

		var by = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
		var result = await _svc.GenerateAsync(request, by);
		return CreatedAtAction(nameof(GetById), new { id = result.ReportID }, result);
	}

	// POST /api/reports/collect  — triggers an immediate data collection from all other services
	// Collects all 4 product lines right now; no need to wait for the 4-hour scheduler cycle.
	[HttpPost("collect")]
	public async Task<IActionResult> CollectNow(CancellationToken ct)
	{
		var today   = DateTime.UtcNow.Date;
		var results = new List<object>();

		foreach (var pl in ProductLines)
		{
			try
			{
				await _collector.CollectAndSnapshotAsync(
					scope:       "Product",
					scopeValue:  pl,
					periodStart: today,
					periodEnd:   today.AddDays(1).AddTicks(-1),
					ct:          ct);

				results.Add(new { productLine = pl, status = "ok" });
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				results.Add(new { productLine = pl, status = "error", error = ex.Message });
			}
		}

		return Ok(new { collectedAt = DateTime.UtcNow, results });
	}

	// GET /api/reports/metrics/hit-ratio?scope=Product&from=2025-01-01&to=2025-12-31
	[HttpGet("metrics/hit-ratio")]
	public async Task<IActionResult> GetHitRatio(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetHitRatioAsync(scope, from, to));

	// GET /api/reports/metrics/tat
	[HttpGet("metrics/tat")]
	public async Task<IActionResult> GetTat(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetTatAsync(scope, from, to));

	// GET /api/reports/metrics/referral-rate
	[HttpGet("metrics/referral-rate")]
	public async Task<IActionResult> GetReferralRate(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetReferralRateAsync(scope, from, to));

	// GET /api/reports/metrics/premium-distribution
	[HttpGet("metrics/premium-distribution")]
	public async Task<IActionResult> GetPremiumDistribution(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetPremiumDistributionAsync(scope, from, to));

	// GET /api/reports/metrics/risk-mix
	[HttpGet("metrics/risk-mix")]
	public async Task<IActionResult> GetRiskMix(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetRiskMixAsync(scope, from, to));

	// GET /api/reports/metrics/uw-productivity
	[HttpGet("metrics/uw-productivity")]
	public async Task<IActionResult> GetUWProductivity(
		[FromQuery] string? scope, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
		=> Ok(await _svc.GetUWProductivityAsync(scope, from, to));
}