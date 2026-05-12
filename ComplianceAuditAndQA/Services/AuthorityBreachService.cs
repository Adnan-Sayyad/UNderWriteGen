using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Models;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Services
{
    public class AuthorityBreachService : IAuthorityBreachService
    {
        private readonly ComplianceDbContext                 _context;
        private readonly ISubmissionClientService            _submissionClient;
        private readonly ILogger<AuthorityBreachService>     _logger;

        public AuthorityBreachService(
            ComplianceDbContext              context,
            ISubmissionClientService         submissionClient,
            ILogger<AuthorityBreachService>  logger)
        {
            _context          = context;
            _submissionClient = submissionClient;
            _logger           = logger;
        }

        // ── GET all ───────────────────────────────────────────────
        public async Task<IEnumerable<AuthorityBreachDto>> GetAllAsync()
        {
            var list = await _context.AuthorityBreaches
                .AsNoTracking()
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        // ── GET by ID ─────────────────────────────────────────────
        public async Task<AuthorityBreachDto> GetByIdAsync(Guid breachId)
        {
            var breach = await _context.AuthorityBreaches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BreachId == breachId && !b.IsDeleted)
                ?? throw new KeyNotFoundException("Authority breach not found.");

            return MapToDto(breach);
        }

        // ── GET by SubmissionId ───────────────────────────────────
        public async Task<IEnumerable<AuthorityBreachDto>> GetBySubmissionIdAsync(Guid submissionId)
        {
            var list = await _context.AuthorityBreaches
                .AsNoTracking()
                .Where(b => b.SubmissionId == submissionId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            if (!list.Any())
                throw new KeyNotFoundException(
                    $"No authority breaches found for submission '{submissionId}'.");

            return list.Select(MapToDto);
        }

        // ── GET by BreachType ─────────────────────────────────────
        public async Task<IEnumerable<AuthorityBreachDto>> GetByBreachTypeAsync(string breachType)
        {
            var list = await _context.AuthorityBreaches
                .AsNoTracking()
                .Where(b => b.BreachType == breachType && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            if (!list.Any())
                throw new KeyNotFoundException(
                    $"No authority breaches found for type '{breachType}'.");

            return list.Select(MapToDto);
        }

        // ── CREATE ────────────────────────────────────────────────
        public async Task<AuthorityBreachDto> CreateAsync(CreateAuthorityBreachDto dto)
        {
            // Best-effort lookup: the frontend already validated the submission ID.
            // We never block creation here — the Submission API may be unavailable,
            // or the user may legitimately log a breach against an archived submission.
            var submissionFound = await _submissionClient.SubmissionExistsAsync(dto.SubmissionId);
            if (!submissionFound)
                _logger.LogWarning(
                    "Submission {Id} not confirmed by Submission API — breach logged anyway.",
                    dto.SubmissionId);

            var breach = new AuthorityBreach
            {
                BreachId     = Guid.NewGuid(),
                SubmissionId = dto.SubmissionId,
                BreachType   = dto.BreachType,
                Description  = dto.Description,
                Status       = "Pending",
                CreatedAt    = DateTime.UtcNow
            };

            await _context.AuthorityBreaches.AddAsync(breach);
            await _context.SaveChangesAsync();
            return MapToDto(breach);
        }

        // ── UPDATE ────────────────────────────────────────────────
        public async Task<AuthorityBreachDto> UpdateAsync(
            Guid breachId, UpdateAuthorityBreachDto dto)
        {
            var breach = await _context.AuthorityBreaches
                .FirstOrDefaultAsync(b => b.BreachId == breachId && !b.IsDeleted)
                ?? throw new KeyNotFoundException("Authority breach not found.");

            breach.BreachType   = dto.BreachType;
            breach.Description  = dto.Description;
            breach.ApprovedBy   = dto.ApprovedBy;
            breach.ApprovedDate = dto.ApprovedDate;
            breach.UpdatedAt    = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(breach);
        }

        // ── PATCH status / details ────────────────────────────────
        public async Task<AuthorityBreachDto> UpdateStatusAsync(
            Guid breachId, UpdateBreachStatusDto dto)
        {
            var breach = await _context.AuthorityBreaches
                .FirstOrDefaultAsync(b => b.BreachId == breachId && !b.IsDeleted)
                ?? throw new KeyNotFoundException("Authority breach not found.");

            breach.Status    = dto.Status;
            breach.UpdatedAt = DateTime.UtcNow;

            // Always persist ApprovedBy so edits can set or clear it.
            breach.ApprovedBy = dto.ApprovedBy;

            // ApprovedDate logic:
            //   • If caller provided a date → use it (normalise to UTC).
            //   • If status is Approved/Rejected and no date → default to now.
            //   • If status is Pending and no date → clear it.
            if (dto.ApprovedDate.HasValue)
                breach.ApprovedDate = DateTime.SpecifyKind(dto.ApprovedDate.Value, DateTimeKind.Utc);
            else if (dto.Status is "Approved" or "Rejected")
                breach.ApprovedDate = DateTime.UtcNow;
            else
                breach.ApprovedDate = null;

            await _context.SaveChangesAsync();
            return MapToDto(breach);
        }

        // ── Map ───────────────────────────────────────────────────
        private static AuthorityBreachDto MapToDto(AuthorityBreach b) => new()
        {
            BreachId     = b.BreachId,
            SubmissionId = b.SubmissionId,
            BreachType   = b.BreachType,
            Description  = b.Description,
            ApprovedBy   = b.ApprovedBy,
            ApprovedDate = b.ApprovedDate.HasValue
                           ? DateTime.SpecifyKind(b.ApprovedDate.Value, DateTimeKind.Utc)
                           : null,
            Status       = b.Status,
            CreatedAt    = DateTime.SpecifyKind(b.CreatedAt, DateTimeKind.Utc),
            UpdatedAt    = b.UpdatedAt.HasValue
                           ? DateTime.SpecifyKind(b.UpdatedAt.Value, DateTimeKind.Utc)
                           : null
        };
    }
}
