using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Models;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Services
{
    public class ExceptionLogService : IExceptionLogService
    {
        private readonly ComplianceDbContext              _context;
        private readonly ISubmissionClientService         _submissionClient;
        private readonly ILogger<ExceptionLogService>     _logger;

        public ExceptionLogService(
            ComplianceDbContext            context,
            ISubmissionClientService       submissionClient,
            ILogger<ExceptionLogService>   logger)
        {
            _context          = context;
            _submissionClient = submissionClient;
            _logger           = logger;
        }

        // ── GET all ───────────────────────────────────────────────
        public async Task<IEnumerable<ExceptionLogDto>> GetAllAsync()
        {
            var list = await _context.ExceptionLogs
                .AsNoTracking()
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.LoggedDate)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        // ── GET by ID ─────────────────────────────────────────────
        public async Task<ExceptionLogDto> GetByIdAsync(Guid exceptionId)
        {
            var log = await _context.ExceptionLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExceptionId == exceptionId && !e.IsDeleted)
                ?? throw new KeyNotFoundException("Exception log not found.");

            return MapToDto(log);
        }

        // ── GET by SubmissionId ───────────────────────────────────
        public async Task<IEnumerable<ExceptionLogDto>> GetBySubmissionIdAsync(Guid submissionId)
        {
            var list = await _context.ExceptionLogs
                .AsNoTracking()
                .Where(e => e.SubmissionId == submissionId && !e.IsDeleted)
                .OrderByDescending(e => e.LoggedDate)
                .ToListAsync();

            if (!list.Any())
                throw new KeyNotFoundException(
                    $"No exception logs found for submission '{submissionId}'.");

            return list.Select(MapToDto);
        }

        // ── GET by Category ───────────────────────────────────────
        public async Task<IEnumerable<ExceptionLogDto>> GetByCategoryAsync(string category)
        {
            var list = await _context.ExceptionLogs
                .AsNoTracking()
                .Where(e => e.Category == category && !e.IsDeleted)
                .OrderByDescending(e => e.LoggedDate)
                .ToListAsync();

            if (!list.Any())
                throw new KeyNotFoundException(
                    $"No exception logs found for category '{category}'.");

            return list.Select(MapToDto);
        }

        // ── CREATE ────────────────────────────────────────────────
        public async Task<ExceptionLogDto> CreateAsync(CreateExceptionLogDto dto)
        {
            // Best-effort lookup — never block. Frontend already validated the ID.
            if (!await _submissionClient.SubmissionExistsAsync(dto.SubmissionId))
                _logger.LogWarning(
                    "Submission {Id} not confirmed by Submission API — exception logged anyway.",
                    dto.SubmissionId);

            var log = new ExceptionLog
            {
                ExceptionId  = Guid.NewGuid(),
                SubmissionId = dto.SubmissionId,
                Category     = dto.Category,
                Details      = dto.Details,
                LoggedDate   = DateTime.UtcNow,
                Status       = "Open",
                CreatedAt    = DateTime.UtcNow
            };

            await _context.ExceptionLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return MapToDto(log);
        }

        // ── UPDATE ────────────────────────────────────────────────
        public async Task<ExceptionLogDto> UpdateAsync(
            Guid exceptionId, UpdateExceptionLogDto dto)
        {
            var log = await _context.ExceptionLogs
                .FirstOrDefaultAsync(e => e.ExceptionId == exceptionId && !e.IsDeleted)
                ?? throw new KeyNotFoundException("Exception log not found.");

            log.Category  = dto.Category;
            log.Details   = dto.Details;
            log.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(log);
        }

        // ── PATCH status ──────────────────────────────────────────
        public async Task<ExceptionLogDto> UpdateStatusAsync(
            Guid exceptionId, UpdateExceptionStatusDto dto)
        {
            var log = await _context.ExceptionLogs
                .FirstOrDefaultAsync(e => e.ExceptionId == exceptionId && !e.IsDeleted)
                ?? throw new KeyNotFoundException("Exception log not found.");

            log.Status    = dto.Status;
            log.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(log);
        }

        // ── Map ───────────────────────────────────────────────────
        private static ExceptionLogDto MapToDto(ExceptionLog e) => new()
        {
            ExceptionId  = e.ExceptionId,
            SubmissionId = e.SubmissionId,
            Category     = e.Category,
            Details      = e.Details,
            LoggedDate   = DateTime.SpecifyKind(e.LoggedDate, DateTimeKind.Utc),
            Status       = e.Status,
            CreatedAt    = DateTime.SpecifyKind(e.CreatedAt, DateTimeKind.Utc),
            UpdatedAt    = e.UpdatedAt.HasValue
                             ? DateTime.SpecifyKind(e.UpdatedAt.Value, DateTimeKind.Utc)
                             : null
        };
    }
}
