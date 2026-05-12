using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DistributionAndPartyManagement.Models.DTOs;
using DistributionAndPartyManagement.Services.Interfaces;

namespace DistributionAndPartyManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class AgentsController : ControllerBase
	{
		private readonly IAgentService _agentService;

		public AgentsController(IAgentService agentService)
		{
			_agentService = agentService;
		}

		// GET api/agents/AGT-20260415-0001  — all authenticated roles
		[HttpGet("{agentId}")]
		public async Task<IActionResult> GetById(string agentId)
		{
			var result = await _agentService.GetByIdAsync(agentId);
			return Ok(ApiResponse<AgentResponseDto>.Ok(result));
		}

		// GET api/agents/by-producer-code/PC-20260415-0001  — all authenticated roles
		[HttpGet("by-producer-code/{producerCode}")]
		public async Task<IActionResult> GetByProducerCode(string producerCode)
		{
			var result = await _agentService.GetByProducerCodeAsync(producerCode);
			return Ok(ApiResponse<AgentResponseDto>.Ok(result));
		}

		// GET api/agents/search  — all authenticated roles
		[HttpGet("search")]
		public async Task<IActionResult> Search(
			[FromQuery] string? name,
			[FromQuery] string? region,
			[FromQuery] string? status)
		{
			var result = await _agentService.SearchAsync(name, region, status);
			return Ok(ApiResponse<List<AgentResponseDto>>.Ok(result));
		}

		// POST api/agents  — Admin only
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateAgentDto dto)
		{
			if (!IsAdmin()) return Forbid();
			var result = await _agentService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { agentId = result.AgentID },
				ApiResponse<AgentResponseDto>.Ok(result, "Agent created successfully."));
		}

		// PUT api/agents/AGT-20260415-0001  — Admin only
		[HttpPut("{agentId}")]
		public async Task<IActionResult> Update(string agentId, [FromBody] UpdateAgentDto dto)
		{
			if (!IsAdmin()) return Forbid();
			var result = await _agentService.UpdateAsync(agentId, dto);
			return Ok(ApiResponse<AgentResponseDto>.Ok(result, "Agent updated successfully."));
		}

		// PATCH api/agents/AGT-20260415-0001/activate  — Admin only
		[HttpPatch("{agentId}/activate")]
		public async Task<IActionResult> Activate(string agentId)
		{
			if (!IsAdmin()) return Forbid();
			var result = await _agentService.ActivateAsync(agentId);
			return Ok(ApiResponse<AgentResponseDto>.Ok(result, "Agent activated."));
		}

		// PATCH api/agents/AGT-20260415-0001/deactivate  — Admin only
		[HttpPatch("{agentId}/deactivate")]
		public async Task<IActionResult> Deactivate(string agentId)
		{
			if (!IsAdmin()) return Forbid();
			var result = await _agentService.DeactivateAsync(agentId);
			return Ok(ApiResponse<AgentResponseDto>.Ok(result, "Agent deactivated."));
		}

		private bool IsAdmin() =>
			User.FindFirstValue(ClaimTypes.Role) == "Admin";
	}
}
