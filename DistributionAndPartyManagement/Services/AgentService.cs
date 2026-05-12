using DistributionAndPartyManagement.Middleware;
using DistributionAndPartyManagement.Models.DTOs;
using DistributionAndPartyManagement.Models.Entities;
using DistributionAndPartyManagement.Repositories.Interfaces;
using DistributionAndPartyManagement.Services.Interfaces;

namespace DistributionAndPartyManagement.Services
{
	public class AgentService : IAgentService
	{
		private readonly IAgentRepository _repo;

		public AgentService(IAgentRepository repo)
		{
			_repo = repo;
		}

		public async Task<AgentResponseDto> GetByIdAsync(string agentId)
		{
			var agent = await _repo.GetByIdAsync(agentId);
			if (agent == null)
				throw new NotFoundException($"Agent with ID '{agentId}' not found.");

			return MapToDto(agent);
		}

		public async Task<AgentResponseDto> GetByProducerCodeAsync(string producerCode)
		{
			var agent = await _repo.GetByProducerCodeAsync(producerCode);
			if (agent == null)
				throw new NotFoundException($"Agent with ProducerCode '{producerCode}' not found.");

			return MapToDto(agent);
		}

		public async Task<List<AgentResponseDto>> SearchAsync(string? name, string? region, string? status)
		{
			var agents = await _repo.SearchAsync(name, region, status);
			return agents.Select(MapToDto).ToList();
		}

		public async Task<AgentResponseDto> CreateAsync(CreateAgentDto dto)
		{
			var count = await _repo.GetCountAsync();
			var sequence = (count + 1).ToString("D4");
			var date = DateTime.UtcNow.ToString("yyyyMMdd");

			var agent = new Agent
			{
				AgentID = $"AGT-{date}-{sequence}",
				Name = dto.Name.Trim(),
				ProducerCode = $"PC-{date}-{sequence}",
				ContactInfo = dto.ContactInfo?.Trim(),
				Region = dto.Region?.Trim(),
				Status = "Active"
			};

			await _repo.AddAsync(agent);
			return MapToDto(agent);
		}

		public async Task<AgentResponseDto> UpdateAsync(string agentId, UpdateAgentDto dto)
		{
			var agent = await _repo.GetByIdAsync(agentId);
			if (agent == null)
				throw new NotFoundException($"Agent with ID '{agentId}' not found.");

			agent.Name = dto.Name.Trim();
			agent.ContactInfo = dto.ContactInfo?.Trim();
			agent.Region = dto.Region?.Trim();

			await _repo.SaveChangesAsync();
			return MapToDto(agent);
		}

		public async Task<AgentResponseDto> ActivateAsync(string agentId)
		{
			var agent = await _repo.GetByIdAsync(agentId);
			if (agent == null)
				throw new NotFoundException($"Agent with ID '{agentId}' not found.");

			if (agent.Status == "Active")
				throw new BusinessRuleException("Agent is already active.");

			agent.Status = "Active";
			await _repo.SaveChangesAsync();
			return MapToDto(agent);
		}

		public async Task<AgentResponseDto> DeactivateAsync(string agentId)
		{
			var agent = await _repo.GetByIdAsync(agentId);
			if (agent == null)
				throw new NotFoundException($"Agent with ID '{agentId}' not found.");

			if (agent.Status == "Inactive")
				throw new BusinessRuleException("Agent is already inactive.");

			agent.Status = "Inactive";
			await _repo.SaveChangesAsync();
			return MapToDto(agent);
		}

		private static AgentResponseDto MapToDto(Agent agent)
		{
			return new AgentResponseDto
			{
				AgentID = agent.AgentID,
				Name = agent.Name,
				ProducerCode = agent.ProducerCode,
				ContactInfo = agent.ContactInfo,
				Region = agent.Region,
				Status = agent.Status
			};
		}
	}
}
