using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/submissions")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _service;

        public SubmissionsController(ISubmissionService service)
        {
            _service = service;
        }

        // GET /api/submissions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var submissions = await _service.GetAllSubmissionsAsync();
            return Ok(submissions);
        }

        // GET /api/submissions/{submissionId}
        [HttpGet("{submissionId:guid}")]
        public async Task<IActionResult> GetById(Guid submissionId)
        {
            var submission = await _service.GetSubmissionByIdAsync(submissionId);
            if (submission is null) return NotFound();
            return Ok(submission);
        }

        // GET /api/submissions/party/{partyId}
        [HttpGet("party/{partyId:guid}")]
        public async Task<IActionResult> GetByPartyId(Guid partyId)
        {
            var submissions = await _service.GetSubmissionsByPartyIdAsync(partyId);
            return Ok(submissions);
        }

        // GET /api/submissions/agent/{agentId}
        [HttpGet("agent/{agentId:guid}")]
        public async Task<IActionResult> GetByAgentId(Guid agentId)
        {
            var submissions = await _service.GetSubmissionsByAgentIdAsync(agentId);
            return Ok(submissions);
        }

        // GET /api/submissions/product-line/{line}
        [HttpGet("product-line/{line}")]
        public async Task<IActionResult> GetByProductLine(ProductLine line)
        {
            var submissions = await _service.GetSubmissionsByProductLineAsync(line);
            return Ok(submissions);
        }

        // POST /api/submissions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
        {
            var created = await _service.CreateSubmissionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { submissionId = created.SubmissionID }, created);
        }

        // PUT /api/submissions/{submissionId}
        [HttpPut("{submissionId:guid}")]
        public async Task<IActionResult> Update(Guid submissionId, [FromBody] UpdateSubmissionDto dto)
        {
            var updated = await _service.UpdateSubmissionAsync(submissionId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/submissions/{submissionId}/status
        [HttpPatch("{submissionId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid submissionId, [FromBody] UpdateSubmissionStatusDto dto)
        {
            var updated = await _service.UpdateSubmissionStatusAsync(submissionId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/submissions/{submissionId}
        [HttpDelete("{submissionId:guid}")]
        public async Task<IActionResult> Delete(Guid submissionId)
        {
            var deleted = await _service.DeleteSubmissionAsync(submissionId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
