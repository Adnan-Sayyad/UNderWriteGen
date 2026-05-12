using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using static RulesScoringAndReferralMatrix.DTOs.PaginationHelpers;

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

        // GET /api/risk-scores/{submissionId}/history?page=0&size=20
        [HttpGet("{submissionId:guid}/history")]
        public async Task<IActionResult> GetHistoryBySubmissionId(
            Guid submissionId,
            [FromQuery] int? page,
            [FromQuery] int? size)
        {
            var (p, s) = Normalize(page, size);
            var result = await _service.GetScoresBySubmissionPagedAsync(submissionId, p, s);
            return Ok(result);
        }

        // GET /api/risk-scores/band/{band}?page=0&size=20
        [HttpGet("band/{band}")]
        public async Task<IActionResult> GetByBand(
            Band band,
            [FromQuery] int? page,
            [FromQuery] int? size)
        {
            var (p, s) = Normalize(page, size);
            var result = await _service.GetScoresByBandPagedAsync(band, p, s);
            return Ok(result);
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
