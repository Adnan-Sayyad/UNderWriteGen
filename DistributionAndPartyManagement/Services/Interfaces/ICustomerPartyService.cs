using DistributionAndPartyManagement.Models.DTOs;

namespace DistributionAndPartyManagement.Services.Interfaces
{
	public interface ICustomerPartyService
	{
		Task<CustomerPartyResponseDto> GetByIdAsync(string partyId);
		Task<List<CustomerPartyResponseDto>> SearchAsync(string? name, string? partyType, string? segment, string? status);
		Task<List<CustomerPartyResponseDto>> CheckDuplicatesAsync(string name);
		Task<CustomerPartyResponseDto> CreateAsync(CreateCustomerPartyDto dto);
		Task<CustomerPartyResponseDto> UpdateAsync(string partyId, UpdateCustomerPartyDto dto);
		Task<CustomerPartyResponseDto> ActivateAsync(string partyId);
		Task<CustomerPartyResponseDto> DeactivateAsync(string partyId);
	}
}
