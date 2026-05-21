using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/questionnaires")]
    [Authorize]
    public class QuestionnairesController : ControllerBase
    {
        private readonly IQuestionnaireService _service;

        public QuestionnairesController(IQuestionnaireService service)
        {
            _service = service;
        }

        // GET /api/questionnaires/{submissionId}
        [HttpGet("{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var questionnaires = await _service.GetQuestionnairesBySubmissionIdAsync(submissionId);
            if (!questionnaires.Any())
                return NotFound(new { message = $"No questionnaire found for submission '{submissionId}'." });
            return Ok(questionnaires);
        }

        // POST /api/questionnaires
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuestionnaireDto dto)
        {
            var created = await _service.CreateQuestionnaireAsync(dto);
            return CreatedAtAction(nameof(GetBySubmissionId), new { submissionId = created.SubmissionID }, created);
        }

        // PUT /api/questionnaires/{qId}
        [HttpPut("{qId:guid}")]
        public async Task<IActionResult> Update(Guid qId, [FromBody] UpdateQuestionnaireDto dto)
        {
            var updated = await _service.UpdateQuestionnaireAsync(qId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/questionnaires/{qId}/status
        [HttpPatch("{qId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid qId, [FromBody] UpdateQuestionnaireStatusDto dto)
        {
            var updated = await _service.UpdateQuestionnaireStatusAsync(qId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // GET /api/questionnaires/templates
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _service.GetTemplatesAsync();
            return Ok(templates);
        }

        // GET /api/questionnaires/templates/{version}
        [HttpGet("templates/{version}")]
        public async Task<IActionResult> GetTemplateByVersion(string version)
        {
            var template = await _service.GetTemplateByVersionAsync(version);
            if (template is null) return NotFound();
            return Ok(template);
        }
    }
}
