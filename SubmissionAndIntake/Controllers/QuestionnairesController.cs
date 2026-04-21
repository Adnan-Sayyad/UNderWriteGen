using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnairesController : ControllerBase
    {
        private readonly IQuestionnaireService _service;

        public QuestionnairesController(IQuestionnaireService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questionnaires = await _service.GetAllQuestionnairesAsync();
            return Ok(questionnaires);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var questionnaire = await _service.GetQuestionnaireByIdAsync(id);
            if (questionnaire is null) return NotFound();
            return Ok(questionnaire);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var questionnaires = await _service.GetQuestionnairesBySubmissionIdAsync(submissionId);
            return Ok(questionnaires);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuestionnaireDto dto)
        {
            var created = await _service.CreateQuestionnaireAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.QID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionnaireDto dto)
        {
            var updated = await _service.UpdateQuestionnaireAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteQuestionnaireAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
