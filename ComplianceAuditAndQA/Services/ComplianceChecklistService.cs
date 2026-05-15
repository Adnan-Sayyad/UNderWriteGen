using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Models;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Services
{
    public class ComplianceChecklistService : IComplianceChecklistService
    {
        private readonly ComplianceDbContext                    _context;
        private readonly ISubmissionClientService               _submissionClient;
        private readonly INotificationClientService             _notifications;
        private readonly ILogger<ComplianceChecklistService>    _logger;

        public ComplianceChecklistService(
            ComplianceDbContext                 context,
            ISubmissionClientService            submissionClient,
            INotificationClientService          notifications,
            ILogger<ComplianceChecklistService> logger)
        {
            _context          = context;
            _submissionClient = submissionClient;
            _notifications    = notifications;
            _logger           = logger;
        }

        // ── GET all ───────────────────────────────────────────────
        public async Task<IEnumerable<ComplianceChecklistDto>> GetAllAsync()
        {
            var list = await _context.ComplianceChecklists
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        // ── GET by ID ─────────────────────────────────────────────
        public async Task<ComplianceChecklistDto> GetByIdAsync(Guid checklistId)
        {
            var checklist = await _context.ComplianceChecklists
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChecklistId == checklistId && !c.IsDeleted)
                ?? throw new KeyNotFoundException("Compliance checklist not found.");

            return MapToDto(checklist);
        }

        // ── GET by SubmissionId ───────────────────────────────────
        public async Task<ComplianceChecklistDto> GetBySubmissionIdAsync(Guid submissionId)
        {
            var checklist = await _context.ComplianceChecklists
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SubmissionId == submissionId && !c.IsDeleted)
                ?? throw new KeyNotFoundException(
                    $"No checklist found for submission '{submissionId}'.");

            return MapToDto(checklist);
        }

        // ── CREATE ────────────────────────────────────────────────
        public async Task<ComplianceChecklistDto> CreateAsync(CreateComplianceChecklistDto dto)
        {
            // Best-effort lookup — never block. Frontend already validated the ID.
            if (!await _submissionClient.SubmissionExistsAsync(dto.SubmissionId))
                _logger.LogWarning(
                    "Submission {Id} not confirmed by Submission API — checklist created anyway.",
                    dto.SubmissionId);

            var checklist = new ComplianceChecklist
            {
                ChecklistId   = Guid.NewGuid(),
                SubmissionId  = dto.SubmissionId,
                ItemsJson     = dto.ItemsJson,
                CompletedBy   = dto.CompletedBy,
                CompletedDate = dto.CompletedDate,
                Status        = dto.Status,
                CreatedAt     = DateTime.UtcNow
            };

            await _context.ComplianceChecklists.AddAsync(checklist);
            await _context.SaveChangesAsync();

            // Checklist created → UWAssistant works the items, Compliance tracks progress.
            var message = $"Compliance checklist created for submission '{dto.SubmissionId}'.";
            _ = _notifications.BroadcastAsync("UWAssistant", message, "Compliance");
            _ = _notifications.BroadcastAsync("Compliance",  message, "Compliance");

            return MapToDto(checklist);
        }

        // ── UPDATE ────────────────────────────────────────────────
        public async Task<ComplianceChecklistDto> UpdateAsync(
            Guid checklistId, UpdateComplianceChecklistDto dto)
        {
            var checklist = await _context.ComplianceChecklists
                .FirstOrDefaultAsync(c => c.ChecklistId == checklistId && !c.IsDeleted)
                ?? throw new KeyNotFoundException("Compliance checklist not found.");

            checklist.ItemsJson     = dto.ItemsJson;
            checklist.CompletedBy   = dto.CompletedBy;
            checklist.CompletedDate = dto.CompletedDate;
            checklist.UpdatedAt     = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(checklist);
        }

        // ── PATCH status ──────────────────────────────────────────
        public async Task<ComplianceChecklistDto> UpdateStatusAsync(
            Guid checklistId, UpdateChecklistStatusDto dto)
        {
            var checklist = await _context.ComplianceChecklists
                .FirstOrDefaultAsync(c => c.ChecklistId == checklistId && !c.IsDeleted)
                ?? throw new KeyNotFoundException("Compliance checklist not found.");

            checklist.Status    = dto.Status;
            checklist.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "Completed" && checklist.CompletedDate == null)
                checklist.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.Status is "Completed")
            {
                // Auto-advance submission to Issued — policy is now in force.
                _ = _submissionClient.UpdateStatusAsync(checklist.SubmissionId, "Issued");

                var message = $"Compliance checklist '{checklist.ChecklistId}' for submission '{checklist.SubmissionId}' is Completed. Policy is now Issued.";
                _ = _notifications.BroadcastAsync("Compliance",  message, "Compliance");
                _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
                _ = _notifications.BroadcastAsync("Agent",       message, "Compliance");
                _ = _notifications.BroadcastAsync("Operations",  message, "Compliance");
            }

            return MapToDto(checklist);
        }

        // ── Map ───────────────────────────────────────────────────
        private static ComplianceChecklistDto MapToDto(ComplianceChecklist c) => new()
        {
            ChecklistId   = c.ChecklistId,
            SubmissionId  = c.SubmissionId,
            ItemsJson     = c.ItemsJson,
            CompletedBy   = c.CompletedBy,
            CompletedDate = c.CompletedDate.HasValue
                              ? DateTime.SpecifyKind(c.CompletedDate.Value, DateTimeKind.Utc)
                              : null,
            Status        = c.Status,
            CreatedAt     = DateTime.SpecifyKind(c.CreatedAt, DateTimeKind.Utc),
            UpdatedAt     = c.UpdatedAt.HasValue
                              ? DateTime.SpecifyKind(c.UpdatedAt.Value, DateTimeKind.Utc)
                              : null
        };
    }
}
