using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Models;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Services
{
    public class AuthorityBreachService : IAuthorityBreachService
    {
        private readonly ComplianceDbContext _context;

        public AuthorityBreachService(ComplianceDbContext context)
            => _context = context;

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

        // ── PATCH status ──────────────────────────────────────────
        public async Task<AuthorityBreachDto> UpdateStatusAsync(
            Guid breachId, UpdateBreachStatusDto dto)
        {
            var breach = await _context.AuthorityBreaches
                .FirstOrDefaultAsync(b => b.BreachId == breachId && !b.IsDeleted)
                ?? throw new KeyNotFoundException("Authority breach not found.");

            breach.Status    = dto.Status;
            breach.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "Approved")
            {
                breach.ApprovedBy   = dto.ApprovedBy;
                breach.ApprovedDate = dto.ApprovedDate ?? DateTime.UtcNow;
            }

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
            ApprovedDate = b.ApprovedDate,
            Status       = b.Status,
            CreatedAt    = b.CreatedAt,
            UpdatedAt    = b.UpdatedAt
        };
    }
}
