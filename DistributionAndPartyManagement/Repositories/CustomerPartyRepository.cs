using Microsoft.EntityFrameworkCore;
using DistributionAndPartyManagement.Data;
using DistributionAndPartyManagement.Models.Entities;
using DistributionAndPartyManagement.Repositories.Interfaces;

namespace DistributionAndPartyManagement.Repositories
{
	public class CustomerPartyRepository : ICustomerPartyRepository
	{
		private readonly AppDbContext _context;

		public CustomerPartyRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<CustomerParty?> GetByIdAsync(string partyId)
		{
			return await _context.CustomerParties.FindAsync(partyId);
		}

		public async Task<List<CustomerParty>> SearchAsync(
			string? name, string? partyType, string? segment, string? status, string? createdByUserId = null)
		{
			var query = _context.CustomerParties.AsQueryable();

			if (!string.IsNullOrWhiteSpace(createdByUserId))
				query = query.Where(c => c.CreatedByUserId == createdByUserId);

			if (!string.IsNullOrWhiteSpace(name))
				query = query.Where(c => c.Name.Contains(name));

			if (!string.IsNullOrWhiteSpace(partyType))
				query = query.Where(c => c.PartyType == partyType);

			if (!string.IsNullOrWhiteSpace(segment))
				query = query.Where(c => c.Segment == segment);

			if (!string.IsNullOrWhiteSpace(status))
				query = query.Where(c => c.Status == status);

			return await query
				.OrderBy(c => c.Status == "Active" ? 0 : 1)
				.ThenBy(c => c.Name)
				.ToListAsync();
		}

		public async Task<List<CustomerParty>> CheckDuplicatesAsync(string name)
		{
			return await _context.CustomerParties
				.Where(c => c.Name == name)
				.ToListAsync();
		}

		public async Task<int> GetCountAsync()
		{
			return await _context.CustomerParties.CountAsync();
		}

		public async Task<CustomerParty> AddAsync(CustomerParty customer)
		{
			_context.CustomerParties.Add(customer);
			await _context.SaveChangesAsync();
			return customer;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
