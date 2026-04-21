using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _service;

        public AttachmentsController(IAttachmentService service)
        {
            _service = service;
        }

        // GET /api/attachments/{submissionId}
        [HttpGet("{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var attachments = await _service.GetAttachmentsBySubmissionIdAsync(submissionId);
            return Ok(attachments);
        }

        // GET /api/attachments/file/{attachmentId}
        [HttpGet("file/{attachmentId:guid}")]
        public async Task<IActionResult> GetById(Guid attachmentId)
        {
            var attachment = await _service.GetAttachmentByIdAsync(attachmentId);
            if (attachment is null) return NotFound();
            return Ok(attachment);
        }

        // GET /api/attachments/{submissionId}/type/{docType}
        [HttpGet("{submissionId:guid}/type/{docType}")]
        public async Task<IActionResult> GetBySubmissionIdAndDocType(Guid submissionId, DocType docType)
        {
            var all = await _service.GetAttachmentsBySubmissionIdAsync(submissionId);
            var filtered = all.Where(a => a.DocType == docType);
            return Ok(filtered);
        }

        // POST /api/attachments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttachmentDto dto)
        {
            var created = await _service.CreateAttachmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { attachmentId = created.AttachmentID }, created);
        }

        // DELETE /api/attachments/{attachmentId}
        [HttpDelete("{attachmentId:guid}")]
        public async Task<IActionResult> Delete(Guid attachmentId)
        {
            var deleted = await _service.DeleteAttachmentAsync(attachmentId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
