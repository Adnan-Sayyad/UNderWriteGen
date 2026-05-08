using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationsAndAlerts.Models.DTOs;
using NotificationsAndAlerts.Services.Interfaces;
using System.Security.Claims;

namespace NotificationsAndAlerts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly IConfiguration _config;

        public NotificationsController(INotificationService service, IConfiguration config)
        {
            _service = service;
            _config  = config;
        }

        // ── CREATE ───────────────────────────────────────────────────────────
        // Two callers are allowed:
        //   1. Internal microservices  → must send the X-Service-Key header.
        //   2. Authenticated users     → must hold one of the creator roles:
        //        Admin | Underwriter | UWManager | UWAssistant | Compliance | Operations
        //
        // Agents and PricingAnalysts are RECEIVERS only — they cannot create.
        // AllowAnonymous is kept so the JWT middleware does not reject internal
        // service calls (which carry no JWT). The manual guard below enforces
        // the real access control.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            string senderEmail;

            if (IsServiceCall())
            {
                senderEmail = "system@uwpro.internal";
            }
            else if (IsAuthorizedRole())
            {
                var mail = GetCurrentMail();
                if (mail is null)
                    return Unauthorized(ApiResponse<object>.Fail("Email claim not found in token."));
                senderEmail = mail;
            }
            else
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Access denied. Only authorised roles (Underwriter, UWManager, UWAssistant, " +
                    "Compliance, Operations, Admin) may create notifications."));
            }

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid input."));

            var result = await _service.CreateAsync(dto, senderEmail);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.NotificationID },
                ApiResponse<NotificationResponseDto>.Ok(result, "Notification created."));
        }

        // ── GET MY NOTIFICATIONS ─────────────────────────────────────────────
        // Returns every notification where the caller is either the recipient
        // OR the sender — so both sides of a conversation can see it.
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var mail = GetCurrentMail();
            if (mail is null)
                return Unauthorized(ApiResponse<object>.Fail("Mail claim not found in token."));

            var list = await _service.GetByParticipantAsync(mail);
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(list));
        }

        // ── UNREAD COUNT ─────────────────────────────────────────────────────
        // Bell-badge count: only notifications received by this user (not sent).
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var mail = GetCurrentMail();
            if (mail is null)
                return Unauthorized(ApiResponse<object>.Fail("Mail claim not found in token."));

            var list  = await _service.GetByParticipantAsync(mail);
            var count = list.Count(n => n.Status == "Unread" && n.RecipientEmail == mail);
            return Ok(ApiResponse<int>.Ok(count));
        }

        // ── GET BY ID ────────────────────────────────────────────────────────
        // Accessible only if the caller is the sender, the recipient, or Admin.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var n = await _service.GetByIdAsync(id);
            if (n is null)
                return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found."));

            if (!CanAccess(n))
                return StatusCode(403, ApiResponse<object>.Fail("You can only access your own notifications."));

            return Ok(ApiResponse<NotificationResponseDto>.Ok(n));
        }

        // ── MARK AS READ ─────────────────────────────────────────────────────
        // Only the recipient (or Admin) may mark a notification as read.
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(string id)
        {
            var n = await _service.GetByIdAsync(id);
            if (n is null)
                return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found."));

            if (!IsRecipientOrAdmin(n))
                return StatusCode(403, ApiResponse<object>.Fail("Only the recipient may mark a notification as read."));

            await _service.MarkAsReadAsync(id);
            return Ok(ApiResponse<string>.Ok(id, "Marked as read."));
        }

        // ── DISMISS ──────────────────────────────────────────────────────────
        // Only the recipient (or Admin) may dismiss a notification.
        [HttpPut("{id}/dismiss")]
        public async Task<IActionResult> Dismiss(string id)
        {
            var n = await _service.GetByIdAsync(id);
            if (n is null)
                return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found."));

            if (!IsRecipientOrAdmin(n))
                return StatusCode(403, ApiResponse<object>.Fail("Only the recipient may dismiss a notification."));

            await _service.DismissAsync(id);
            return Ok(ApiResponse<string>.Ok(id, "Notification dismissed."));
        }

        // ── DELETE ───────────────────────────────────────────────────────────
        // Both the sender and the recipient (or Admin) may delete a notification.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var n = await _service.GetByIdAsync(id);
            if (n is null)
                return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found."));

            if (!CanAccess(n))
                return StatusCode(403, ApiResponse<object>.Fail("You can only delete your own notifications."));

            await _service.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok(id, "Notification deleted."));
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────────────

        private string? GetCurrentMail() =>
            User.FindFirst(ClaimTypes.Email)?.Value;

        // True when the caller is the sender OR the recipient OR an Admin.
        private bool CanAccess(NotificationResponseDto n)
        {
            if (User.IsInRole("Admin")) return true;
            var me = GetCurrentMail();
            return me == n.RecipientEmail || me == n.SenderEmail;
        }

        // True when the caller is the recipient OR an Admin (for read/dismiss).
        private bool IsRecipientOrAdmin(NotificationResponseDto n) =>
            User.IsInRole("Admin") || GetCurrentMail() == n.RecipientEmail;

        // Validates the X-Service-Key header sent by other microservices.
        private bool IsServiceCall()
        {
            var incoming   = Request.Headers["X-Service-Key"].ToString();
            var configured = _config["InternalServiceKey"];
            return !string.IsNullOrEmpty(configured) && incoming == configured;
        }

        // Roles that are permitted to CREATE notifications manually.
        // Agents and PricingAnalysts are excluded — they are recipients only.
        private bool IsAuthorizedRole() =>
            User.Identity?.IsAuthenticated == true &&
            (User.IsInRole("Admin")       ||
             User.IsInRole("Underwriter") ||
             User.IsInRole("UWManager")   ||
             User.IsInRole("UWAssistant") ||
             User.IsInRole("Compliance")  ||
             User.IsInRole("Operations"));
    }
}
