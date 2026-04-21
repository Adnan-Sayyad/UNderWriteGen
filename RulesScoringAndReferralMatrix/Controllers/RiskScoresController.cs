using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/risk-scores")]
    public class RiskScoresController : ControllerBase
    {
        private readonly IRiskScoreService _service;

        public RiskScoresController(IRiskScoreService service)
        {
            _service = service;
        }

        // GET /api/risk-scores/{submissionId}
        [HttpGet("{submissionId:guid}")]
        public async Task<IActionResult> GetLatestBySubmissionId(Guid submissionId)
        {
            var score = await _service.GetLatestScoreBySubmissionIdAsync(submissionId);
            if (score is null) return NotFound();
            return Ok(score);
        }

        // GET /api/risk-scores/{submissionId}/history
        [HttpGet("{submissionId:guid}/history")]
        public async Task<IActionResult> GetHistoryBySubmissionId(Guid submissionId)
        {
            var scores = await _service.GetScoresBySubmissionIdAsync(submissionId);
            return Ok(scores);
        }

        // GET /api/risk-scores/band/{band}
        [HttpGet("band/{band}")]
        public async Task<IActionResult> GetByBand(Band band)
        {
            var scores = await _service.GetScoresByBandAsync(band);
            return Ok(scores);
        }

        // POST /api/risk-scores/calculate/{submissionId}
        [HttpPost("calculate/{submissionId:guid}")]
        public async Task<IActionResult> Calculate(Guid submissionId)
        {
            var score = await _service.CalculateScoreForSubmissionAsync(submissionId);
            return Ok(score);
        }
    }
}
