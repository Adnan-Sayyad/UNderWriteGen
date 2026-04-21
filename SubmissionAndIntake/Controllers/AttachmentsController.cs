using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _service;

        public AttachmentsController(IAttachmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var attachments = await _service.GetAllAttachmentsAsync();
            return Ok(attachments);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var attachment = await _service.GetAttachmentByIdAsync(id);
            if (attachment is null) return NotFound();
            return Ok(attachment);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var attachments = await _service.GetAttachmentsBySubmissionIdAsync(submissionId);
            return Ok(attachments);
        }

        [HttpGet("doctype/{docType}")]
        public async Task<IActionResult> GetByDocType(DocType docType)
        {
            var attachments = await _service.GetAttachmentsByDocTypeAsync(docType);
            return Ok(attachments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttachmentDto dto)
        {
            var created = await _service.CreateAttachmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.AttachmentID }, created);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAttachmentAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
