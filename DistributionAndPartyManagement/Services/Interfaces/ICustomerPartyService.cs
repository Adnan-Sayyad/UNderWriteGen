using DistributionAndPartyManagement.Models.DTOs;

namespace DistributionAndPartyManagement.Services.Interfaces
{
	public interface ICustomerPartyService
	{
		Task<CustomerPartyResponseDto> GetByIdAsync(string partyId);
		Task<List<CustomerPartyResponseDto>> SearchAsync(string? name, string? partyType, string? segment, string? status, string? createdByUserId = null);
		Task<List<CustomerPartyResponseDto>> CheckDuplicatesAsync(string name);
		Task<CustomerPartyResponseDto> CreateAsync(CreateCustomerPartyDto dto, string createdByUserId);
		Task<CustomerPartyResponseDto> UpdateAsync(string partyId, UpdateCustomerPartyDto dto, string currentUserId, bool isAdmin);
		Task<CustomerPartyResponseDto> ActivateAsync(string partyId);
		Task<CustomerPartyResponseDto> DeactivateAsync(string partyId);
	}
}
