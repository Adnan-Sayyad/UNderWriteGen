using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using static RulesScoringAndReferralMatrix.DTOs.PaginationHelpers;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/risk-scores")]
    [Authorize(Roles = "Underwriter,UWAssistant,Admin")]
    public class RiskScoresController : ControllerBase
    {
        private readonly IRiskScoreService _service;
        private readonly IConfiguration _config;

        public RiskScoresController(IRiskScoreService service, IConfiguration config)
        {
            _service = service;
            _config  = config;
        }

        // GET /api/risk-scores/{submissionId}
        // Accepts JWT (UW/Admin) OR X-Internal-Service-Key (Pricing service).
        [HttpGet("{submissionId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestBySubmissionId(Guid submissionId)
        {
            var internalKey = _config["InternalServiceKey"];
            var headerKey   = Request.Headers["X-Internal-Service-Key"].FirstOrDefault();
            var isInternal  = !string.IsNullOrEmpty(internalKey) && headerKey == internalKey;
            var isAuthed    = User.Identity?.IsAuthenticated == true;
            if (!isInternal && !isAuthed) return Unauthorized();

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
