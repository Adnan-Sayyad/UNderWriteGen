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

        public SubmissionService(ISubmissionRepository repository, IDistributionValidationService distributionValidation)
        {
            _repository = repository;
            _distributionValidation = distributionValidation;
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

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAgentIdAsync(Guid agentId)
        {
            var submissions = await _repository.GetByAgentIdAsync(agentId);
            return submissions.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByPartyIdAsync(Guid partyId)
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
            if (!await _distributionValidation.AgentExistsAsync(dto.AgentID.ToString()))
                throw new KeyNotFoundException($"Agent '{dto.AgentID}' not found in Distribution service.");
            if (!await _distributionValidation.PartyExistsAsync(dto.PartyID.ToString()))
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
            return updated is null ? null : MapToResponseDto(updated);
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
