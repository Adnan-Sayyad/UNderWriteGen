using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralService _service;

        public ReferralsController(IReferralService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var referrals = await _service.GetAllReferralsAsync();
            return Ok(referrals);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var referral = await _service.GetReferralByIdAsync(id);
            if (referral is null) return NotFound();
            return Ok(referral);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var referrals = await _service.GetReferralsBySubmissionIdAsync(submissionId);
            return Ok(referrals);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(ReferralStatus status)
        {
            var referrals = await _service.GetReferralsByStatusAsync(status);
            return Ok(referrals);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReferralDto dto)
        {
            var created = await _service.CreateReferralAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ReferralID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReferralDto dto)
        {
            var updated = await _service.UpdateReferralAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }
    }
}
