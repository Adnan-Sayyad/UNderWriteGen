using Microsoft.AspNetCore.Mvc;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Services;

namespace UnderwritingWorkflowAndDecisions.Controllers
{
    [ApiController]
    [Route("api/subjectivities")]
    public class SubjectivitiesController : ControllerBase
    {
        private readonly ISubjectivityService _service;

        public SubjectivitiesController(ISubjectivityService service) => _service = service;

        [HttpGet("{submissionId:guid}")]
        public IActionResult GetBySubmission(Guid submissionId) =>
            Ok(_service.GetBySubmission(submissionId));

        [HttpGet("detail/{subjectivityId:guid}")]
        public IActionResult GetById(Guid subjectivityId)
        {
            var item = _service.GetById(subjectivityId);
            return item is null ? NotFound(new { message = $"Subjectivity {subjectivityId} not found." }) : Ok(item);
        }

        [HttpGet("due/{date}")]
        public IActionResult GetDueBy(string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
            return Ok(_service.GetDueBy(parsedDate));
        }

        [HttpPost]
        public IActionResult Add([FromBody] CreateSubjectivityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Description is required." });

            var item = _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { subjectivityId = item.SubjectivityID }, item);
        }

        [HttpPut("{subjectivityId:guid}")]
        public IActionResult Update(Guid subjectivityId, [FromBody] UpdateSubjectivityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Description is required." });

            var updated = _service.Update(subjectivityId, dto);
            return updated is null ? NotFound(new { message = $"Subjectivity {subjectivityId} not found." }) : Ok(updated);
        }

        [HttpPatch("{subjectivityId:guid}/status")]
        public IActionResult UpdateStatus(Guid subjectivityId, [FromBody] UpdateSubjectivityStatusDto dto)
        {
            var validStatuses = new[] { "Open", "Met", "Waived" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid status. Valid: {string.Join(", ", validStatuses)}" });

            var updated = _service.UpdateStatus(subjectivityId, dto);
            return updated is null ? NotFound(new { message = $"Subjectivity {subjectivityId} not found." }) : Ok(updated);
        }

        [HttpDelete("{subjectivityId:guid}")]
        public IActionResult Delete(Guid subjectivityId)
        {
            if (!_service.Delete(subjectivityId))
                return NotFound(new { message = $"Subjectivity {subjectivityId} not found." });
            return NoContent();
        }
    }
}
