using Microsoft.AspNetCore.Mvc;
using NotificationsAndAlerts.Models.DTOs;
using NotificationsAndAlerts.Services.Interfaces;

namespace NotificationsAndAlerts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        // POST /api/notifications
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid input"));

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.NotificationID },
                ApiResponse<NotificationResponseDto>.Ok(result, "Notification created"));
        }

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(list));
        }

        // GET /api/notifications/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var n = await _service.GetByIdAsync(id);
            if (n is null)
                return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found"));
            return Ok(ApiResponse<NotificationResponseDto>.Ok(n));
        }

        // GET /api/notifications/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var list = await _service.GetByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(list));
        }

        // PUT /api/notifications/{id}/read
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var ok = await _service.MarkAsReadAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found"));
            return Ok(ApiResponse<int>.Ok(id, "Marked as read"));
        }

        // PUT /api/notifications/{id}/dismiss
        [HttpPut("{id:int}/dismiss")]
        public async Task<IActionResult> Dismiss(int id)
        {
            var ok = await _service.DismissAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found"));
            return Ok(ApiResponse<int>.Ok(id, "Notification dismissed"));
        }

        // DELETE /api/notifications/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.Fail($"Notification '{id}' not found"));
            return Ok(ApiResponse<int>.Ok(id, "Notification deleted"));
        }
    }
}
