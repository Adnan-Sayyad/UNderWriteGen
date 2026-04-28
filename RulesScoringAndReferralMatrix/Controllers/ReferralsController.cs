using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/referrals")]
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralService _service;

        public ReferralsController(IReferralService service)
        {
            _service = service;
        }

        // GET /api/referrals
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var referrals = await _service.GetAllReferralsAsync();
            return Ok(referrals);
        }

        // GET /api/referrals/{referralId}
        [HttpGet("{referralId:guid}")]
        public async Task<IActionResult> GetById(Guid referralId)
        {
            var referral = await _service.GetReferralByIdAsync(referralId);
            if (referral is null) return NotFound();
            return Ok(referral);
        }

        // GET /api/referrals/submission/{submissionId}
        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var referrals = await _service.GetReferralsBySubmissionIdAsync(submissionId);
            return Ok(referrals);
        }

        // GET /api/referrals/authority/{authority}
        [HttpGet("authority/{authority}")]
        public async Task<IActionResult> GetByAuthority(RequiredAuthority authority)
        {
            var referrals = await _service.GetReferralsByAuthorityAsync(authority);
            return Ok(referrals);
        }

        // GET /api/referrals/assigned/{userId}
        [HttpGet("assigned/{userId}")]
        public async Task<IActionResult> GetByAssignedUser(string userId)
        {
            var referrals = await _service.GetReferralsByAssignedToAsync(userId);
            return Ok(referrals);
        }

        // POST /api/referrals
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReferralDto dto)
        {
            var created = await _service.CreateReferralAsync(dto);
            return CreatedAtAction(nameof(GetById), new { referralId = created.ReferralID }, created);
        }

        // PUT /api/referrals/{referralId}
        [HttpPut("{referralId:guid}")]
        public async Task<IActionResult> Update(Guid referralId, [FromBody] UpdateReferralDto dto)
        {
            var updated = await _service.UpdateReferralAsync(referralId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/referrals/{referralId}/status
        [HttpPatch("{referralId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid referralId, [FromBody] UpdateReferralStatusDto dto)
        {
            var updated = await _service.UpdateReferralStatusAsync(referralId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }
    }
}
