using RiskDataAndEvidence.Domain;
using RiskDataAndEvidence.DTOs;
using RiskDataAndEvidence.Repositories;

namespace RiskDataAndEvidence.Services;

public class EvidenceRefService : IEvidenceRefService
{
    private readonly IEvidenceRefRepository _repo;
    private readonly ISubmissionValidationService _submissionValidation;
    private readonly INotificationClientService _notifications;

    public EvidenceRefService(
        IEvidenceRefRepository repo,
        ISubmissionValidationService submissionValidation,
        INotificationClientService notifications)
    {
        _repo = repo;
        _submissionValidation = submissionValidation;
        _notifications = notifications;
    }

    public async Task<IEnumerable<EvidenceRefSummaryDto>> GetBySubmissionIdAsync(Guid submissionId)
        => (await _repo.GetBySubmissionIdAsync(submissionId)).Select(ToSummary);

    public async Task<EvidenceRefDetailDto?> GetByIdAsync(Guid evidenceId)
    {
        var ev = await _repo.GetByIdAsync(evidenceId);
        return ev is null ? null : ToDetail(ev);
    }

    public async Task<IEnumerable<EvidenceRefSummaryDto>> GetBySubmissionAndTypeAsync(Guid submissionId, string evidenceType)
        => (await _repo.GetBySubmissionAndTypeAsync(submissionId, evidenceType)).Select(ToSummary);

    public async Task<EvidenceRefDetailDto> CreateAsync(CreateEvidenceRefDto dto)
    {
        if (!await _submissionValidation.SubmissionExistsAsync(dto.SubmissionID))
            throw new KeyNotFoundException($"Submission '{dto.SubmissionID}' not found.");

        var ev = new EvidenceRef
        {
            SubmissionID = dto.SubmissionID,
            EvidenceType = dto.EvidenceType,
            Provider = dto.Provider,
            ReferenceNo = dto.ReferenceNo,
            ResultJSON = dto.ResultJSON,
            ReceivedDate = dto.ReceivedDate,
            Status = dto.Status
        };
        await _repo.AddAsync(ev);
        await _repo.SaveChangesAsync();

        // PDF §4.4: evidence requested → chase the relevant party.
        // Requested → Agent (provide it); Received → Underwriter (review it).
        var message = $"Evidence {dto.EvidenceType} for submission '{dto.SubmissionID}' is now {dto.Status}.";
        switch (dto.Status)
        {
            case "Requested":
                _ = _notifications.BroadcastAsync("Agent",       message, "Compliance");
                _ = _notifications.BroadcastAsync("UWAssistant", message, "Compliance");
                break;
            case "Received":
                _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
                break;
            case "NotAvailable":
                _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
                _ = _notifications.BroadcastAsync("UWAssistant", message, "Compliance");
                break;
        }

        return ToDetail(ev);
    }

    public async Task<EvidenceRefDetailDto?> UpdateAsync(Guid evidenceId, UpdateEvidenceRefDto dto)
    {
        var ev = await _repo.GetByIdAsync(evidenceId);
        if (ev is null) return null;

        ev.EvidenceType = dto.EvidenceType;
        ev.Provider = dto.Provider;
        ev.ReferenceNo = dto.ReferenceNo;
        ev.ResultJSON = dto.ResultJSON;
        ev.ReceivedDate = dto.ReceivedDate;
        ev.Status = dto.Status;

        await _repo.UpdateAsync(ev);
        await _repo.SaveChangesAsync();
        return ToDetail(ev);
    }

    public async Task<EvidenceRefDetailDto?> UpdateStatusAsync(Guid evidenceId, UpdateEvidenceStatusDto dto)
    {
        var ev = await _repo.GetByIdAsync(evidenceId);
        if (ev is null) return null;

        ev.Status = dto.Status;
        if (dto.ResultJSON is not null) ev.ResultJSON = dto.ResultJSON;
        if (dto.ReceivedDate is not null) ev.ReceivedDate = dto.ReceivedDate;

        await _repo.UpdateAsync(ev);
        await _repo.SaveChangesAsync();

        var message = $"Evidence {ev.EvidenceType} for submission '{ev.SubmissionID}' is now {dto.Status}.";
        if (dto.Status == "Received")
            _ = _notifications.BroadcastAsync("Underwriter", message, "Compliance");
        else if (dto.Status == "NotAvailable")
            _ = _notifications.BroadcastAsync("UWManager", message, "Compliance");

        return ToDetail(ev);
    }

    private static EvidenceRefSummaryDto ToSummary(EvidenceRef e) => new()
    {
        EvidenceID = e.EvidenceID,
        SubmissionID = e.SubmissionID,
        EvidenceType = e.EvidenceType,
        Provider = e.Provider,
        ReferenceNo = e.ReferenceNo,
        ReceivedDate = e.ReceivedDate,
        Status = e.Status
    };

    private static EvidenceRefDetailDto ToDetail(EvidenceRef e) => new()
    {
        EvidenceID = e.EvidenceID,
        SubmissionID = e.SubmissionID,
        EvidenceType = e.EvidenceType,
        Provider = e.Provider,
        ReferenceNo = e.ReferenceNo,
        ResultJSON = e.ResultJSON,
        ReceivedDate = e.ReceivedDate,
        Status = e.Status
    };
}
