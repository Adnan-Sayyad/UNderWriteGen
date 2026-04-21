using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompletenessChecksController : ControllerBase
    {
        private readonly ICompletenessCheckService _service;

        public CompletenessChecksController(ICompletenessCheckService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var checks = await _service.GetAllChecksAsync();
            return Ok(checks);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var check = await _service.GetCheckByIdAsync(id);
            if (check is null) return NotFound();
            return Ok(check);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var checks = await _service.GetChecksBySubmissionIdAsync(submissionId);
            return Ok(checks);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(CheckStatus status)
        {
            var checks = await _service.GetChecksByStatusAsync(status);
            return Ok(checks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompletenessCheckDto dto)
        {
            var created = await _service.CreateCheckAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.CheckID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompletenessCheckDto dto)
        {
            var updated = await _service.UpdateCheckAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }
    }
}
