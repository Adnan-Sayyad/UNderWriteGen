using DistributionAndPartyManagement.Models.Entities;

namespace DistributionAndPartyManagement.Repositories.Interfaces
{
	public interface IAgentRepository
	{
		Task<Agent?> GetByIdAsync(string agentId);
		Task<Agent?> GetByProducerCodeAsync(string producerCode);
		Task<List<Agent>> SearchAsync(string? name, string? region, string? status);
		Task<bool> ProducerCodeExistsAsync(string producerCode);
		Task<int> GetCountAsync();
		Task<Agent> AddAsync(Agent agent);
		Task SaveChangesAsync();
	}
}
