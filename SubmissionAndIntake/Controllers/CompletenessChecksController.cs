using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/completeness-checks")]
    [Authorize]
    public class CompletenessChecksController : ControllerBase
    {
        private readonly ICompletenessCheckService _service;

        public CompletenessChecksController(ICompletenessCheckService service)
        {
            _service = service;
        }

        // GET /api/completeness-checks/{submissionId}
        [HttpGet("{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var checks = await _service.GetChecksBySubmissionIdAsync(submissionId);
            return Ok(checks);
        }

        // POST /api/completeness-checks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompletenessCheckDto dto)
        {
            var created = await _service.CreateCheckAsync(dto);
            return CreatedAtAction(nameof(GetBySubmissionId), new { submissionId = created.SubmissionID }, created);
        }

        // PUT /api/completeness-checks/{checkId}
        [HttpPut("{checkId:guid}")]
        public async Task<IActionResult> Update(Guid checkId, [FromBody] UpdateCompletenessCheckDto dto)
        {
            var updated = await _service.UpdateCheckAsync(checkId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/completeness-checks/{checkId}/status
        [HttpPatch("{checkId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid checkId, [FromBody] UpdateCheckStatusDto dto)
        {
            var updated = await _service.UpdateCheckStatusAsync(checkId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }
    }
}
