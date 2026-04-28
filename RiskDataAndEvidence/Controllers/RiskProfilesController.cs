using Microsoft.AspNetCore.Mvc;
using RiskDataAndEvidence.DTOs;
using RiskDataAndEvidence.Services;

namespace RiskDataAndEvidence.Controllers;

[ApiController]
[Route("api/risk-profiles")]
[Produces("application/json")]
public class RiskProfilesController : ControllerBase
{
    private readonly IRiskProfileService _service;

    public RiskProfilesController(IRiskProfileService service) => _service = service;

    // GET /api/risk-profiles
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RiskProfileSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET /api/risk-profiles/{riskId}
    [HttpGet("{riskId:guid}")]
    [ProducesResponseType(typeof(RiskProfileDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid riskId)
    {
        var result = await _service.GetByIdAsync(riskId);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/risk-profiles/submission/{submissionId}
    [HttpGet("submission/{submissionId:guid}")]
    [ProducesResponseType(typeof(RiskProfileDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySubmission(Guid submissionId)
    {
        var result = await _service.GetBySubmissionIdAsync(submissionId);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/risk-profiles/type/{riskType}
    [HttpGet("type/{riskType}")]
    [ProducesResponseType(typeof(IEnumerable<RiskProfileSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByType(string riskType)
        => Ok(await _service.GetByRiskTypeAsync(riskType));

    // POST /api/risk-profiles
    [HttpPost]
    [ProducesResponseType(typeof(RiskProfileDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRiskProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { riskId = created.RiskID }, created);
    }

    // PUT /api/risk-profiles/{riskId}
    [HttpPut("{riskId:guid}")]
    [ProducesResponseType(typeof(RiskProfileDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid riskId, [FromBody] UpdateRiskProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(riskId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE /api/risk-profiles/{riskId}
    [HttpDelete("{riskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid riskId)
    {
        var deleted = await _service.DeleteAsync(riskId);
        return deleted ? NoContent() : NotFound();
    }
}
