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
			if (await _repo.ProducerCodeExistsAsync(dto.ProducerCode))
				throw new DuplicateException($"ProducerCode '{dto.ProducerCode}' already exists.");

			var agent = new Agent
			{
				AgentID = await GenerateAgentIdAsync(),
				Name = dto.Name,
				ProducerCode = dto.ProducerCode,
				ContactInfo = dto.ContactInfo,
				Region = dto.Region,
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

			agent.Name = dto.Name;
			agent.ContactInfo = dto.ContactInfo;
			agent.Region = dto.Region;

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

		// Auto-generate ID like AGT-20260415-0001
		private async Task<string> GenerateAgentIdAsync()
		{
			var count = await _repo.GetCountAsync();
			var sequence = (count + 1).ToString("D4");
			var date = DateTime.UtcNow.ToString("yyyyMMdd");
			return $"AGT-{date}-{sequence}";
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
