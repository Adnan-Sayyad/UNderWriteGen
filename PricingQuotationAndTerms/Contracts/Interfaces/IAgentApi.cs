using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Contract for calling the Distribution/Party microservice (port 8082).
/// Used to check if an agent is a "preferred agent" for discount calculation.
/// </summary>
public interface IAgentApi
{
    Task<AgentDto?> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default);
}
