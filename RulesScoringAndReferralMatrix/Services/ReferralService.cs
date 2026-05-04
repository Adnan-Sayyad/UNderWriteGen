using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class ReferralService : IReferralService
    {
        private readonly IReferralRepository _repository;
        private readonly INotificationClientService _notificationClient;

        public ReferralService(IReferralRepository repository, INotificationClientService notificationClient)
        {
            _repository = repository;
            _notificationClient = notificationClient;
        }

        public async Task<IEnumerable<ReferralResponseDto>> GetAllReferralsAsync()
        {
            var referrals = await _repository.GetAllAsync();
            return referrals.Select(MapToResponseDto);
        }

        public async Task<ReferralResponseDto?> GetReferralByIdAsync(Guid id)
        {
            var referral = await _repository.GetByIdAsync(id);
            return referral is null ? null : MapToResponseDto(referral);
        }

        public async Task<IEnumerable<ReferralResponseDto>> GetReferralsBySubmissionIdAsync(Guid submissionId)
        {
            var referrals = await _repository.GetBySubmissionIdAsync(submissionId);
            return referrals.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ReferralResponseDto>> GetReferralsByStatusAsync(ReferralStatus status)
        {
            var referrals = await _repository.GetByStatusAsync(status);
            return referrals.Select(MapToResponseDto);
        }

        public async Task<ReferralResponseDto> CreateReferralAsync(CreateReferralDto dto)
        {
            var referral = new Referral
            {
                SubmissionID = dto.SubmissionID,
                RaisedBy = dto.RaisedBy,
                Reason = dto.Reason,
                RequiredAuthority = dto.RequiredAuthority,
                AssignedTo = dto.AssignedTo
            };

            var created = await _repository.CreateAsync(referral);

            if (!string.IsNullOrWhiteSpace(dto.AssignedTo))
            {
                var message = $"New referral for submission '{dto.SubmissionID}' requires your review. Authority: {dto.RequiredAuthority}. Reason: {dto.Reason}";
                await _notificationClient.SendAsync(dto.AssignedTo, message, "Referral");
            }

            return MapToResponseDto(created);
        }

        public async Task<IEnumerable<ReferralResponseDto>> GetReferralsByAuthorityAsync(RequiredAuthority authority)
        {
            var referrals = await _repository.GetByAuthorityAsync(authority);
            return referrals.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ReferralResponseDto>> GetReferralsByAssignedToAsync(string userId)
        {
            var referrals = await _repository.GetByAssignedToAsync(userId);
            return referrals.Select(MapToResponseDto);
        }

        public async Task<ReferralResponseDto?> UpdateReferralAsync(Guid id, UpdateReferralDto dto)
        {
            var referral = new Referral
            {
                ReferralID = id,
                AssignedTo = dto.AssignedTo,
                Status = dto.Status
            };

            var updated = await _repository.UpdateAsync(referral);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<ReferralResponseDto?> UpdateReferralStatusAsync(Guid id, UpdateReferralStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            return updated is null ? null : MapToResponseDto(updated);
        }

        private static ReferralResponseDto MapToResponseDto(Referral referral) => new()
        {
            ReferralID = referral.ReferralID,
            SubmissionID = referral.SubmissionID,
            RaisedBy = referral.RaisedBy,
            Reason = referral.Reason,
            RequiredAuthority = referral.RequiredAuthority,
            AssignedTo = referral.AssignedTo,
            CreatedDate = referral.CreatedDate,
            Status = referral.Status
        };
    }
}
