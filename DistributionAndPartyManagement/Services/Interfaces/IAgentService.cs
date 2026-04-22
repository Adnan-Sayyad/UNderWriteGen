using DistributionAndPartyManagement.Models.DTOs;

namespace DistributionAndPartyManagement.Services.Interfaces
{
	public interface IAgentService
	{
		Task<AgentResponseDto> GetByIdAsync(string agentId);
		Task<AgentResponseDto> GetByProducerCodeAsync(string producerCode);
		Task<List<AgentResponseDto>> SearchAsync(string? name, string? region, string? status);
		Task<AgentResponseDto> CreateAsync(CreateAgentDto dto);
		Task<AgentResponseDto> UpdateAsync(string agentId, UpdateAgentDto dto);
		Task<AgentResponseDto> ActivateAsync(string agentId);
		Task<AgentResponseDto> DeactivateAsync(string agentId);
	}
}
