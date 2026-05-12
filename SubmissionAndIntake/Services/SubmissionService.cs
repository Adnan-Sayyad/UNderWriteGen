using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionRepository _repository;
        private readonly IDistributionValidationService _distributionValidation;
        private readonly INotificationClientService _notifications;

        public SubmissionService(
            ISubmissionRepository repository,
            IDistributionValidationService distributionValidation,
            INotificationClientService notifications)
        {
            _repository = repository;
            _distributionValidation = distributionValidation;
            _notifications = notifications;
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync()
        {
            var submissions = await _repository.GetAllAsync();
            return submissions.Select(MapToResponseDto);
        }

        public async Task<SubmissionResponseDto?> GetSubmissionByIdAsync(Guid id)
        {
            var submission = await _repository.GetByIdAsync(id);
            return submission is null ? null : MapToResponseDto(submission);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAgentIdAsync(string agentId)
        {
            var submissions = await _repository.GetByAgentIdAsync(agentId);
            return submissions.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByPartyIdAsync(string partyId)
        {
            var submissions = await _repository.GetByPartyIdAsync(partyId);
            return submissions.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByStatusAsync(SubmissionStatus status)
        {
            var submissions = await _repository.GetByStatusAsync(status);
            return submissions.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByProductLineAsync(ProductLine productLine)
        {
            var submissions = await _repository.GetByProductLineAsync(productLine);
            return submissions.Select(MapToResponseDto);
        }

        public async Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto)
        {
            if (!await _distributionValidation.AgentExistsAsync(dto.AgentID))
                throw new KeyNotFoundException($"Agent '{dto.AgentID}' not found in Distribution service.");
            if (!await _distributionValidation.PartyExistsAsync(dto.PartyID))
                throw new KeyNotFoundException($"Party '{dto.PartyID}' not found in Distribution service.");

            var submission = new Submission
            {
                PartyID = dto.PartyID,
                AgentID = dto.AgentID,
                ProductLine = dto.ProductLine,
                CoverageJSON = dto.CoverageJSON,
                InceptionDate = dto.InceptionDate
            };

            var created = await _repository.CreateAsync(submission);

            // PDF §2.11/§4.11: new submission notification — underwriters should pick up incoming work
            _ = _notifications.BroadcastAsync(
                "Underwriter",
                $"New {created.ProductLine} submission '{created.SubmissionID}' has been created and needs underwriting review.",
                "Referral");

            return MapToResponseDto(created);
        }

        public async Task<SubmissionResponseDto?> UpdateSubmissionAsync(Guid id, UpdateSubmissionDto dto)
        {
            var submission = new Submission
            {
                SubmissionID = id,
                PartyID = dto.PartyID,
                AgentID = dto.AgentID,
                ProductLine = dto.ProductLine,
                CoverageJSON = dto.CoverageJSON,
                InceptionDate = dto.InceptionDate,
                Status = dto.Status
            };

            var updated = await _repository.UpdateAsync(submission);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<SubmissionResponseDto?> UpdateSubmissionStatusAsync(Guid id, UpdateSubmissionStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            if (updated is null) return null;

            // Notify the right people for each status transition.
            var msg = $"Submission '{updated.SubmissionID}' is now {dto.Status}.";
            switch (dto.Status)
            {
                case SubmissionStatus.UnderReview:
                    _ = _notifications.BroadcastAsync("Underwriter", msg, "Referral");
                    break;
                case SubmissionStatus.IntakeComplete:
                    _ = _notifications.BroadcastAsync("UWAssistant", msg, "Referral");
                    break;
                case SubmissionStatus.Quoted:
                    _ = _notifications.BroadcastAsync("Agent", $"Quote available for submission '{updated.SubmissionID}'.", "Quote");
                    break;
                case SubmissionStatus.Declined:
                    _ = _notifications.BroadcastAsync("Agent", $"Submission '{updated.SubmissionID}' has been declined.", "Quote");
                    break;
            }

            return MapToResponseDto(updated);
        }

        public async Task<bool> DeleteSubmissionAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static SubmissionResponseDto MapToResponseDto(Submission submission) => new()
        {
            SubmissionID = submission.SubmissionID,
            PartyID = submission.PartyID,
            AgentID = submission.AgentID,
            ProductLine = submission.ProductLine,
            CoverageJSON = submission.CoverageJSON,
            InceptionDate = submission.InceptionDate,
            CreatedDate = submission.CreatedDate,
            Status = submission.Status
        };
    }
}
