using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/submissions")]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _service;
        private readonly IConfiguration _config;

        public SubmissionsController(ISubmissionService service, IConfiguration config)
        {
            _service = service;
            _config  = config;
        }

        // GET /api/submissions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var submissions = await _service.GetAllSubmissionsAsync();
            return Ok(submissions);
        }

        // GET /api/submissions/{submissionId}
        // Accepts either a valid JWT OR the X-Internal-Service-Key header
        // (used by Pricing, Policy, and other internal services).
        [HttpGet("{submissionId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid submissionId)
        {
            var internalKey = _config["InternalServiceKey"];
            var headerKey   = Request.Headers["X-Internal-Service-Key"].FirstOrDefault();
            var isInternal  = !string.IsNullOrEmpty(internalKey) && headerKey == internalKey;
            var isAuthed    = User.Identity?.IsAuthenticated == true;

            if (!isInternal && !isAuthed)
                return Unauthorized();

            var submission = await _service.GetSubmissionByIdAsync(submissionId);
            if (submission is null) return NotFound();
            return Ok(submission);
        }

        // GET /api/submissions/party/{partyId}
        [HttpGet("party/{partyId}")]
        public async Task<IActionResult> GetByPartyId(string partyId)
        {
            var submissions = await _service.GetSubmissionsByPartyIdAsync(partyId);
            return Ok(submissions);
        }

        // GET /api/submissions/agent/{agentId}
        [HttpGet("agent/{agentId}")]
        public async Task<IActionResult> GetByAgentId(string agentId)
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
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
        {
            try
            {
                var created = await _service.CreateSubmissionAsync(dto);
                return CreatedAtAction(nameof(GetById), new { submissionId = created.SubmissionID }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
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
        [AllowAnonymous]
        public async Task<IActionResult> UpdateStatus(Guid submissionId, [FromBody] UpdateSubmissionStatusDto dto)
        {
            var internalKey = _config["InternalServiceKey"];
            var headerKey   = Request.Headers["X-Internal-Service-Key"].FirstOrDefault();
            var isInternal  = !string.IsNullOrEmpty(internalKey) && headerKey == internalKey;
            var isAuthed    = User.Identity?.IsAuthenticated == true;

            if (!isInternal && !isAuthed)
                return Unauthorized();

            var updated = await _service.UpdateSubmissionStatusAsync(submissionId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/submissions/{submissionId}
        [HttpDelete("{submissionId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid submissionId)
        {
            var deleted = await _service.DeleteSubmissionAsync(submissionId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
