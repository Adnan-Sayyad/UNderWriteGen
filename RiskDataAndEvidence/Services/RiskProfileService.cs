using RiskDataAndEvidence.Domain;
using RiskDataAndEvidence.DTOs;
using RiskDataAndEvidence.Repositories;

namespace RiskDataAndEvidence.Services;

public class RiskProfileService : IRiskProfileService
{
    private readonly IRiskProfileRepository _repo;

    public RiskProfileService(IRiskProfileRepository repo) => _repo = repo;

    public async Task<IEnumerable<RiskProfileSummaryDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(ToSummary);

    public async Task<RiskProfileDetailDto?> GetByIdAsync(Guid riskId)
    {
        var profile = await _repo.GetByIdAsync(riskId);
        return profile is null ? null : ToDetail(profile);
    }

    public async Task<RiskProfileDetailDto?> GetBySubmissionIdAsync(Guid submissionId)
    {
        var profile = await _repo.GetBySubmissionIdAsync(submissionId);
        return profile is null ? null : ToDetail(profile);
    }

    public async Task<IEnumerable<RiskProfileSummaryDto>> GetByRiskTypeAsync(string riskType)
        => (await _repo.GetByRiskTypeAsync(riskType)).Select(ToSummary);

    public async Task<RiskProfileDetailDto> CreateAsync(CreateRiskProfileDto dto)
    {
        var profile = new RiskProfile
        {
            SubmissionID = dto.SubmissionID,
            RiskType = dto.RiskType,
            AttributesJSON = dto.AttributesJSON,
            RiskNotes = dto.RiskNotes,
            LastUpdated = DateTime.UtcNow
        };
        await _repo.AddAsync(profile);
        await _repo.SaveChangesAsync();
        return ToDetail(profile);
    }

    public async Task<RiskProfileDetailDto?> UpdateAsync(Guid riskId, UpdateRiskProfileDto dto)
    {
        var profile = await _repo.GetByIdAsync(riskId);
        if (profile is null) return null;

        profile.RiskType = dto.RiskType;
        profile.AttributesJSON = dto.AttributesJSON;
        profile.RiskNotes = dto.RiskNotes;
        profile.LastUpdated = DateTime.UtcNow;

        await _repo.UpdateAsync(profile);
        await _repo.SaveChangesAsync();
        return ToDetail(profile);
    }

    public async Task<bool> DeleteAsync(Guid riskId)
    {
        var profile = await _repo.GetByIdAsync(riskId);
        if (profile is null) return false;
        await _repo.DeleteAsync(profile);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static RiskProfileSummaryDto ToSummary(RiskProfile r) => new()
    {
        RiskID = r.RiskID,
        SubmissionID = r.SubmissionID,
        RiskType = r.RiskType,
        RiskNotes = r.RiskNotes,
        LastUpdated = r.LastUpdated
    };

    private static RiskProfileDetailDto ToDetail(RiskProfile r) => new()
    {
        RiskID = r.RiskID,
        SubmissionID = r.SubmissionID,
        RiskType = r.RiskType,
        AttributesJSON = r.AttributesJSON,
        RiskNotes = r.RiskNotes,
        LastUpdated = r.LastUpdated
    };
}
