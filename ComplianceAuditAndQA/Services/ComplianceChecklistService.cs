using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Models;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Services
{
    public class ComplianceChecklistService : IComplianceChecklistService
    {
        private readonly ComplianceDbContext _context;

        public ComplianceChecklistService(ComplianceDbContext context)
            => _context = context;

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
            return MapToDto(checklist);
        }

        // ── Map ───────────────────────────────────────────────────
        private static ComplianceChecklistDto MapToDto(ComplianceChecklist c) => new()
        {
            ChecklistId   = c.ChecklistId,
            SubmissionId  = c.SubmissionId,
            ItemsJson     = c.ItemsJson,
            CompletedBy   = c.CompletedBy,
            CompletedDate = c.CompletedDate,
            Status        = c.Status,
            CreatedAt     = c.CreatedAt,
            UpdatedAt     = c.UpdatedAt
        };
    }
}
