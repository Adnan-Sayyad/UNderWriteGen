using Microsoft.EntityFrameworkCore;
using DistributionAndPartyManagement.Data;
using DistributionAndPartyManagement.Models.Entities;
using DistributionAndPartyManagement.Repositories.Interfaces;

namespace DistributionAndPartyManagement.Repositories
{
	public class AgentRepository : IAgentRepository
	{
		private readonly AppDbContext _context;

		public AgentRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<Agent?> GetByIdAsync(string agentId)
		{
			return await _context.Agents.FindAsync(agentId);
		}

		public async Task<Agent?> GetByProducerCodeAsync(string producerCode)
		{
			return await _context.Agents
				.FirstOrDefaultAsync(a => a.ProducerCode == producerCode);
		}

		public async Task<List<Agent>> SearchAsync(string? name, string? region, string? status)
		{
			var query = _context.Agents.AsQueryable();

			if (!string.IsNullOrWhiteSpace(name))
				query = query.Where(a => a.Name.Contains(name));

			if (!string.IsNullOrWhiteSpace(region))
				query = query.Where(a => a.Region != null && a.Region.Contains(region));

			if (!string.IsNullOrWhiteSpace(status))
				query = query.Where(a => a.Status == status);

			return await query
				.OrderBy(a => a.Status == "Active" ? 0 : 1)
				.ThenBy(a => a.Name)
				.ToListAsync();
		}

		public async Task<bool> ProducerCodeExistsAsync(string producerCode)
		{
			return await _context.Agents.AnyAsync(a => a.ProducerCode == producerCode);
		}

		public async Task<int> GetCountAsync()
		{
			return await _context.Agents.CountAsync();
		}

		public async Task<Agent> AddAsync(Agent agent)
		{
			_context.Agents.Add(agent);
			await _context.SaveChangesAsync();
			return agent;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
