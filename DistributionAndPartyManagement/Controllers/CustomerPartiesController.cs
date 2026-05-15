using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DistributionAndPartyManagement.Models.DTOs;
using DistributionAndPartyManagement.Services.Interfaces;
using DistributionAndPartyManagement.Middleware;

namespace DistributionAndPartyManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class CustomerPartiesController : ControllerBase
	{
		private readonly ICustomerPartyService _customerPartyService;

		public CustomerPartiesController(ICustomerPartyService customerPartyService)
		{
			_customerPartyService = customerPartyService;
		}

		// GET api/customerparties/PTY-xxx
		// Admin: any customer | Agent: only own | Other roles: any (read-only)
		[HttpGet("{partyId}")]
		public async Task<IActionResult> GetById(string partyId)
		{
			var result = await _customerPartyService.GetByIdAsync(partyId);

			// Agent may not view customers they didn't create.
			// If CreatedByUserId is null/empty (legacy data before tracking was added), allow access.
			var uid = GetUserId();
			if (GetRole() == "Agent"
				&& !string.IsNullOrEmpty(result.CreatedByUserId)
				&& result.CreatedByUserId != uid)
				throw new NotFoundException($"Customer with ID '{partyId}' not found.");

			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result));
		}

		// GET api/customerparties/search
		// Admin/other roles: all customers | Agent: only own
		[HttpGet("search")]
		public async Task<IActionResult> Search(
			[FromQuery] string? name,
			[FromQuery] string? partyType,
			[FromQuery] string? segment,
			[FromQuery] string? status)
		{
			// When caller is an agent, scope results to their own customers only
			string? ownerFilter = GetRole() == "Agent" ? GetUserId() : null;
			var result = await _customerPartyService.SearchAsync(name, partyType, segment, status, ownerFilter);
			return Ok(ApiResponse<List<CustomerPartyResponseDto>>.Ok(result));
		}

		// GET api/customerparties/check-duplicates  — all authenticated roles
		[HttpGet("check-duplicates")]
		public async Task<IActionResult> CheckDuplicates([FromQuery] string name)
		{
			var result = await _customerPartyService.CheckDuplicatesAsync(name);
			return Ok(ApiResponse<List<CustomerPartyResponseDto>>.Ok(result));
		}

		// POST api/customerparties  — Agent only (Admin cannot create customers)
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateCustomerPartyDto dto)
		{
			if (GetRole() != "Agent") return Forbid();
			var result = await _customerPartyService.CreateAsync(dto, GetUserId());
			return CreatedAtAction(nameof(GetById), new { partyId = result.PartyID },
				ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer created successfully."));
		}

		// PUT api/customerparties/PTY-xxx  — Admin (any) or Agent (own only)
		[HttpPut("{partyId}")]
		public async Task<IActionResult> Update(string partyId, [FromBody] UpdateCustomerPartyDto dto)
		{
			var role = GetRole();
			if (role != "Admin" && role != "Agent") return Forbid();
			var result = await _customerPartyService.UpdateAsync(partyId, dto, GetUserId(), role == "Admin");
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer updated successfully."));
		}

		// PATCH api/customerparties/PTY-xxx/activate  — Admin only
		[HttpPatch("{partyId}/activate")]
		public async Task<IActionResult> Activate(string partyId)
		{
			if (GetRole() != "Admin") return Forbid();
			var result = await _customerPartyService.ActivateAsync(partyId);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer activated."));
		}

		// PATCH api/customerparties/PTY-xxx/deactivate  — Admin only
		[HttpPatch("{partyId}/deactivate")]
		public async Task<IActionResult> Deactivate(string partyId)
		{
			if (GetRole() != "Admin") return Forbid();
			var result = await _customerPartyService.DeactivateAsync(partyId);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer deactivated."));
		}

		// .NET 7+ JsonWebTokenHandler stores claims with their short JWT names ("role", "sub"),
		// not the long ClaimTypes URIs — check both to handle all token handler variants.
		private string GetRole() =>
			User.FindFirstValue("role") ??
			User.FindFirstValue(ClaimTypes.Role) ?? "";

		private string GetUserId() =>
			User.FindFirstValue("sub") ??
			User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
	}
}
