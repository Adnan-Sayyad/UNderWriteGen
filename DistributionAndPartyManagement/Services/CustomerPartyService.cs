using DistributionAndPartyManagement.Middleware;
using DistributionAndPartyManagement.Models.DTOs;
using DistributionAndPartyManagement.Models.Entities;
using DistributionAndPartyManagement.Repositories.Interfaces;
using DistributionAndPartyManagement.Services.Interfaces;

namespace DistributionAndPartyManagement.Services
{
	public class CustomerPartyService : ICustomerPartyService
	{
		private readonly ICustomerPartyRepository _repo;

		public CustomerPartyService(ICustomerPartyRepository repo)
		{
			_repo = repo;
		}

		public async Task<CustomerPartyResponseDto> GetByIdAsync(string partyId)
		{
			var customer = await _repo.GetByIdAsync(partyId);
			if (customer == null)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			return MapToDto(customer);
		}

		public async Task<List<CustomerPartyResponseDto>> SearchAsync(
			string? name, string? partyType, string? segment, string? status)
		{
			var customers = await _repo.SearchAsync(name, partyType, segment, status);
			return customers.Select(MapToDto).ToList();
		}

		public async Task<List<CustomerPartyResponseDto>> CheckDuplicatesAsync(string name)
		{
			var duplicates = await _repo.CheckDuplicatesAsync(name);
			return duplicates.Select(MapToDto).ToList();
		}

		public async Task<CustomerPartyResponseDto> CreateAsync(CreateCustomerPartyDto dto)
		{
			var customer = new CustomerParty
			{
				PartyID = await GeneratePartyIdAsync(),
				PartyType = dto.PartyType.Trim(),
				Name = dto.Name.Trim(),
				DOBIncorporation = dto.DOBIncorporation,
				ContactInfo = dto.ContactInfo?.Trim(),
				Segment = dto.Segment.Trim(),
				Status = "Active"
			};

			await _repo.AddAsync(customer);
			return MapToDto(customer);
		}

		public async Task<CustomerPartyResponseDto> UpdateAsync(string partyId, UpdateCustomerPartyDto dto)
		{
			var customer = await _repo.GetByIdAsync(partyId);
			if (customer == null)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			customer.Name = dto.Name.Trim();
			customer.DOBIncorporation = dto.DOBIncorporation;
			customer.ContactInfo = dto.ContactInfo?.Trim();
			customer.Segment = dto.Segment.Trim();

			await _repo.SaveChangesAsync();
			return MapToDto(customer);
		}

		public async Task<CustomerPartyResponseDto> ActivateAsync(string partyId)
		{
			var customer = await _repo.GetByIdAsync(partyId);
			if (customer == null)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			if (customer.Status == "Active")
				throw new BusinessRuleException("Customer is already active.");

			customer.Status = "Active";
			await _repo.SaveChangesAsync();
			return MapToDto(customer);
		}

		public async Task<CustomerPartyResponseDto> DeactivateAsync(string partyId)
		{
			var customer = await _repo.GetByIdAsync(partyId);
			if (customer == null)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			if (customer.Status == "Inactive")
				throw new BusinessRuleException("Customer is already inactive.");

			customer.Status = "Inactive";
			await _repo.SaveChangesAsync();
			return MapToDto(customer);
		}

		private async Task<string> GeneratePartyIdAsync()
		{
			var count = await _repo.GetCountAsync();
			var sequence = (count + 1).ToString("D4");
			var date = DateTime.UtcNow.ToString("yyyyMMdd");
			return $"PTY-{date}-{sequence}";
		}

		private static CustomerPartyResponseDto MapToDto(CustomerParty c)
		{
			return new CustomerPartyResponseDto
			{
				PartyID = c.PartyID,
				PartyType = c.PartyType,
				Name = c.Name,
				DOBIncorporation = c.DOBIncorporation,
				ContactInfo = c.ContactInfo,
				Segment = c.Segment,
				Status = c.Status
			};
		}
	}
}
