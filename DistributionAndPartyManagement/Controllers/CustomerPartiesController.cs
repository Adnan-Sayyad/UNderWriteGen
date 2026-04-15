using Microsoft.AspNetCore.Mvc;
using DistributionAndPartyManagement.Models.DTOs;
using DistributionAndPartyManagement.Services.Interfaces;

namespace DistributionAndPartyManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CustomerPartiesController : ControllerBase
	{
		private readonly ICustomerPartyService _customerPartyService;

		public CustomerPartiesController(ICustomerPartyService customerPartyService)
		{
			_customerPartyService = customerPartyService;
		}

		// GET api/customerparties/PTY-20260415-0001
		[HttpGet("{partyId}")]
		public async Task<IActionResult> GetById(string partyId)
		{
			var result = await _customerPartyService.GetByIdAsync(partyId);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result));
		}

		// GET api/customerparties/search?name=Amit&segment=Retail
		[HttpGet("search")]
		public async Task<IActionResult> Search(
			[FromQuery] string? name,
			[FromQuery] string? partyType,
			[FromQuery] string? segment,
			[FromQuery] string? status)
		{
			var result = await _customerPartyService.SearchAsync(name, partyType, segment, status);
			return Ok(ApiResponse<List<CustomerPartyResponseDto>>.Ok(result));
		}

		// GET api/customerparties/check-duplicates?name=Amit
		[HttpGet("check-duplicates")]
		public async Task<IActionResult> CheckDuplicates([FromQuery] string name)
		{
			var result = await _customerPartyService.CheckDuplicatesAsync(name);
			return Ok(ApiResponse<List<CustomerPartyResponseDto>>.Ok(result));
		}

		// POST api/customerparties
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateCustomerPartyDto dto)
		{
			var result = await _customerPartyService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { partyId = result.PartyID },
				ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer created successfully."));
		}

		// PUT api/customerparties/PTY-20260415-0001
		[HttpPut("{partyId}")]
		public async Task<IActionResult> Update(string partyId, [FromBody] UpdateCustomerPartyDto dto)
		{
			var result = await _customerPartyService.UpdateAsync(partyId, dto);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer updated successfully."));
		}

		// PATCH api/customerparties/PTY-20260415-0001/activate
		[HttpPatch("{partyId}/activate")]
		public async Task<IActionResult> Activate(string partyId)
		{
			var result = await _customerPartyService.ActivateAsync(partyId);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer activated."));
		}

		// PATCH api/customerparties/PTY-20260415-0001/deactivate
		[HttpPatch("{partyId}/deactivate")]
		public async Task<IActionResult> Deactivate(string partyId)
		{
			var result = await _customerPartyService.DeactivateAsync(partyId);
			return Ok(ApiResponse<CustomerPartyResponseDto>.Ok(result, "Customer deactivated."));
		}
	}
}
