using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Services;

namespace UnderwritingWorkflowAndDecisions.Controllers
{
    [ApiController]
    [Route("api/uw-notes")]
    [Authorize(Roles = "Underwriter,UWAssistant,Admin")]
    public class UWNotesController : ControllerBase
    {
        private readonly IUWNoteService _service;

        public UWNotesController(IUWNoteService service) => _service = service;

        [HttpGet("{submissionId:guid}")]
        public IActionResult GetBySubmission(Guid submissionId) =>
            Ok(_service.GetBySubmission(submissionId));

        [HttpGet("detail/{noteId:guid}")]
        public IActionResult GetById(Guid noteId)
        {
            var note = _service.GetById(noteId);
            return note is null ? NotFound(new { message = $"Note {noteId} not found." }) : Ok(note);
        }

        [HttpGet("author/{authorId:guid}")]
        public IActionResult GetByAuthor(Guid authorId) =>
            Ok(_service.GetByAuthor(authorId));

        [HttpPost]
        public IActionResult Add([FromBody] CreateUWNoteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NoteText))
                return BadRequest(new { message = "NoteText is required." });

            var note = _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { noteId = note.NoteID }, note);
        }

        [HttpPut("{noteId:guid}")]
        public IActionResult Update(Guid noteId, [FromBody] UpdateUWNoteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NoteText))
                return BadRequest(new { message = "NoteText is required." });

            var updated = _service.Update(noteId, dto);
            return updated is null ? NotFound(new { message = $"Note {noteId} not found." }) : Ok(updated);
        }

        [HttpDelete("{noteId:guid}")]
        public IActionResult Delete(Guid noteId)
        {
            if (!_service.Delete(noteId))
                return NotFound(new { message = $"Note {noteId} not found." });
            return NoContent();
        }
    }
}
