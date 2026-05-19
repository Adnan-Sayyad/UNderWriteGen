using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _service;
        private readonly IWebHostEnvironment _env;

        public AttachmentsController(IAttachmentService service, IWebHostEnvironment env)
        {
            _service = service;
            _env     = env;
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

        // POST /api/attachments/upload  (multipart/form-data)
        // Accepts the real file bytes, saves to wwwroot/uploads/, then creates the DB record.
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            [FromForm] Guid   submissionID,
            [FromForm] string docType,
            [FromForm] string uploadedBy,
            IFormFile         file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file provided." });

            // Allowed extensions guard
            var allowedExt = new[] { ".jpg", ".jpeg", ".pdf", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return BadRequest(new { success = false, message = $"File type '{ext}' is not allowed." });

            // Build a safe path:  wwwroot/uploads/{submissionId}/
            var uploadDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                         "uploads", submissionID.ToString());
            Directory.CreateDirectory(uploadDir);

            // Use original filename (sanitised) — prepend docType so it is clearly labelled
            var safeFileName = $"{docType}_{Path.GetFileName(file.FileName)}";
            var filePath     = Path.Combine(uploadDir, safeFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Relative URL that will be served by UseStaticFiles via the proxy
            var fileUri = $"/uploads/{submissionID}/{safeFileName}";

            if (!Enum.TryParse<DocType>(docType, out var parsedDocType))
                return BadRequest(new { success = false, message = $"Unknown docType '{docType}'." });

            var dto = new CreateAttachmentDto
            {
                SubmissionID = submissionID,
                DocType      = parsedDocType,
                FileURI      = fileUri,
                UploadedBy   = uploadedBy,
            };

            var created = await _service.CreateAttachmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { attachmentId = created.AttachmentID }, created);
        }

        // GET /api/attachments/download/{attachmentId}
        // Streams the physical file back to the browser (fallback for direct download).
        [HttpGet("download/{attachmentId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(Guid attachmentId)
        {
            var attachment = await _service.GetAttachmentByIdAsync(attachmentId);
            if (attachment is null) return NotFound();

            // fileUri looks like  /uploads/{submissionId}/{docType}_{filename}
            var relativePath = attachment.FileURI.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath     = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                relativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "File not found on server." });

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            var fileName = Path.GetFileName(fullPath);
            var stream   = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return File(stream, contentType, fileName);
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
