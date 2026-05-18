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
			string? name, string? partyType, string? segment, string? status, string? createdByUserId = null)
		{
			var customers = await _repo.SearchAsync(name, partyType, segmenst, status, createdByUserId);
			return customers.Select(MapToDto).ToList();
		}

		public async Task<List<CustomerPartyResponseDto>> CheckDuplicatesAsync(string name)
		{
			var duplicates = await _repo.CheckDuplicatesAsync(name);
			return duplicates.Select(MapToDto).ToList();
		}

		public async Task<CustomerPartyResponseDto> CreateAsync(CreateCustomerPartyDto dto, string createdByUserId)
		{
			// Use a short GUID segment so the ID is always unique regardless of deletions or concurrency.
			var date      = DateTime.UtcNow.ToString("yyyyMMdd");
			var uniquePart = Guid.NewGuid().ToString("N")[..6].ToUpper(); // e.g. "A3B4C5"

			var customer = new CustomerParty
			{
				PartyID = $"PTY-{date}-{uniquePart}",
				PartyType = dto.PartyType.Trim(),
				Name = dto.Name.Trim(),
				DOBIncorporation = dto.DOBIncorporation,
				ContactInfo = dto.ContactInfo?.Trim(),
				Segment = dto.Segment.Trim(),
				Status = "Active",
				CreatedByUserId = createdByUserId
			};

			await _repo.AddAsync(customer);
			return MapToDto(customer);
		}

		public async Task<CustomerPartyResponseDto> UpdateAsync(
			string partyId, UpdateCustomerPartyDto dto, string currentUserId, bool isAdmin)
		{
			var customer = await _repo.GetByIdAsync(partyId);
			if (customer == null)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			if (!isAdmin && customer.CreatedByUserId != currentUserId)
				throw new ForbiddenException("You can only edit customers you have added.");

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
				Status = c.Status,
				CreatedByUserId = c.CreatedByUserId
			};
		}
	}
}
