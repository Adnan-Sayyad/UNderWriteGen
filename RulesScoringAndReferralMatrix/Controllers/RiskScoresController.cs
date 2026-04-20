using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskScoresController : ControllerBase
    {
        private readonly IRiskScoreService _service;

        public RiskScoresController(IRiskScoreService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var scores = await _service.GetAllScoresAsync();
            return Ok(scores);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var score = await _service.GetScoreByIdAsync(id);
            if (score is null) return NotFound();
            return Ok(score);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var scores = await _service.GetScoresBySubmissionIdAsync(submissionId);
            return Ok(scores);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRiskScoreDto dto)
        {
            var created = await _service.CreateScoreAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.RiskScoreID }, created);
        }
    }
}
