using DistributionAndPartyManagement.Models.Entities;

namespace DistributionAndPartyManagement.Repositories.Interfaces
{
	public interface ICustomerPartyRepository
	{
		Task<CustomerParty?> GetByIdAsync(string partyId);
		Task<List<CustomerParty>> SearchAsync(string? name, string? partyType, string? segment, string? status);
		Task<List<CustomerParty>> CheckDuplicatesAsync(string name);
		Task<int> GetCountAsync();
		Task<CustomerParty> AddAsync(CustomerParty customer);
		Task SaveChangesAsync();
	}
}
