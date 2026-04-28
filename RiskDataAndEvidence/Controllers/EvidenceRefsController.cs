using Microsoft.AspNetCore.Mvc;
using RiskDataAndEvidence.DTOs;
using RiskDataAndEvidence.Services;

namespace RiskDataAndEvidence.Controllers;

[ApiController]
[Route("api/evidence-refs")]
[Produces("application/json")]
public class EvidenceRefsController : ControllerBase
{
    private readonly IEvidenceRefService _service;

    public EvidenceRefsController(IEvidenceRefService service) => _service = service;

    // GET /api/evidence-refs/{submissionId}
    [HttpGet("{submissionId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EvidenceRefSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubmission(Guid submissionId)
        => Ok(await _service.GetBySubmissionIdAsync(submissionId));

    // GET /api/evidence-refs/detail/{evidenceId}
    [HttpGet("detail/{evidenceId:guid}")]
    [ProducesResponseType(typeof(EvidenceRefDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid evidenceId)
    {
        var result = await _service.GetByIdAsync(evidenceId);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/evidence-refs/{submissionId}/type/{evidenceType}
    [HttpGet("{submissionId:guid}/type/{evidenceType}")]
    [ProducesResponseType(typeof(IEnumerable<EvidenceRefSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByType(Guid submissionId, string evidenceType)
        => Ok(await _service.GetBySubmissionAndTypeAsync(submissionId, evidenceType));

    // POST /api/evidence-refs
    [HttpPost]
    [ProducesResponseType(typeof(EvidenceRefDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEvidenceRefDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { evidenceId = created.EvidenceID }, created);
    }

    // PUT /api/evidence-refs/{evidenceId}
    [HttpPut("{evidenceId:guid}")]
    [ProducesResponseType(typeof(EvidenceRefDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid evidenceId, [FromBody] UpdateEvidenceRefDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(evidenceId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    // PATCH /api/evidence-refs/{evidenceId}/status
    [HttpPatch("{evidenceId:guid}/status")]
    [ProducesResponseType(typeof(EvidenceRefDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid evidenceId, [FromBody] UpdateEvidenceStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateStatusAsync(evidenceId, dto);
        return result is null ? NotFound() : Ok(result);
    }
}
