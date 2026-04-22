using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Application.Services;

/// <summary>
/// DEV STUB for the Distribution/Party microservice (port 8082).
/// Every other agent is "preferred" so you can test the discount logic.
/// Replace with HttpAgentApi when teammate's module is ready.
/// </summary>
public class FakeAgentApi : IAgentApi
{
    public Task<AgentDto?> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default)
    {
        // Odd last byte = preferred agent (to test both paths)
        bool isPreferred = agentId.ToByteArray()[15] % 2 == 1;

        return Task.FromResult<AgentDto?>(new AgentDto
        {
            AgentId          = agentId,
            ProducerCode     = "PROD-FAKE-001",
            Region           = "Metro",
            IsPreferredAgent = isPreferred,
            CommissionRate   = 0.05m
        });
    }
}
