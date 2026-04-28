using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.DTOs.Responses;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Domain.Entities;
using PricingQuotationAndTerms.Domain.Repositories;

namespace PricingQuotationAndTerms.Application.Services;

public class PricingParamService : IPricingParamService
{
    private readonly IPricingParamRepository _repo;
    private readonly ILogger<PricingParamService> _logger;

    public PricingParamService(IPricingParamRepository repo, ILogger<PricingParamService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<PricingParamResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.GetAllAsync(ct)).Select(Map);

    public async Task<PricingParamResponse> CreateAsync(
        CreatePricingParamRequest request, CancellationToken ct = default)
    {
        var existing = await _repo.GetActiveParamAsync(request.ProductLine, request.ParamName, ct);
        if (existing != null)
            throw new InvalidOperationException(
                $"Active parameter '{request.ParamName}' already exists for '{request.ProductLine}'. Update it instead.");

        var param = new PricingParam(
            request.ProductLine,
            request.ParamName,
            request.Value,
            request.Description,
            request.EffectiveFrom,
            request.EffectiveTo);

        await _repo.AddAsync(param, ct);
        _logger.LogInformation("PricingParam created: {Line}/{Name}={Value}", param.ProductLine, param.ParamName, param.Value);
        return Map(param);
    }

    public async Task<PricingParamResponse> UpdateAsync(
        Guid paramId, UpdatePricingParamRequest request, CancellationToken ct = default)
    {
        var param = await _repo.GetByIdAsync(paramId, ct)
            ?? throw new KeyNotFoundException($"Pricing parameter '{paramId}' not found.");

        param.UpdateValue(request.Value, request.Description);
        await _repo.UpdateAsync(param, ct);
        _logger.LogInformation("PricingParam updated: {Line}/{Name}→{Value}", param.ProductLine, param.ParamName, param.Value);
        return Map(param);
    }

    public async Task<IEnumerable<PricingParamResponse>> GetByProductLineAsync(
        string productLine, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productLine))
            throw new ArgumentException("Product line cannot be empty.");
        return (await _repo.GetByProductLineAsync(productLine, ct)).Select(Map);
    }

    public async Task<IEnumerable<PricingParamResponse>> GetByEffectiveDateAsync(
        DateTime date, CancellationToken ct = default)
        => (await _repo.GetByEffectiveDateAsync(date, ct)).Select(Map);

    private static PricingParamResponse Map(PricingParam p) => new()
    {
        Id            = p.Id,
        ProductLine   = p.ProductLine,
        ParamName     = p.ParamName,
        Value         = p.Value,
        Description   = p.Description,
        EffectiveFrom = p.EffectiveFrom,
        EffectiveTo   = p.EffectiveTo,
        IsActive      = p.IsActive,
        CreatedAt     = p.CreatedAt,
        UpdatedAt     = p.UpdatedAt
    };
}
