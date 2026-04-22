using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Services
{
    public class CompletenessCheckService : ICompletenessCheckService
    {
        private readonly ICompletenessCheckRepository _repository;

        public CompletenessCheckService(ICompletenessCheckRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CompletenessCheckResponseDto>> GetAllChecksAsync()
        {
            var checks = await _repository.GetAllAsync();
            return checks.Select(MapToResponseDto);
        }

        public async Task<CompletenessCheckResponseDto?> GetCheckByIdAsync(Guid id)
        {
            var check = await _repository.GetByIdAsync(id);
            return check is null ? null : MapToResponseDto(check);
        }

        public async Task<IEnumerable<CompletenessCheckResponseDto>> GetChecksBySubmissionIdAsync(Guid submissionId)
        {
            var checks = await _repository.GetBySubmissionIdAsync(submissionId);
            return checks.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CompletenessCheckResponseDto>> GetChecksByStatusAsync(CheckStatus status)
        {
            var checks = await _repository.GetByStatusAsync(status);
            return checks.Select(MapToResponseDto);
        }

        public async Task<CompletenessCheckResponseDto> CreateCheckAsync(CreateCompletenessCheckDto dto)
        {
            var check = new CompletenessCheck
            {
                SubmissionID = dto.SubmissionID,
                MissingItemsJSON = dto.MissingItemsJSON
            };

            var created = await _repository.CreateAsync(check);
            return MapToResponseDto(created);
        }

        public async Task<CompletenessCheckResponseDto?> UpdateCheckAsync(Guid id, UpdateCompletenessCheckDto dto)
        {
            var check = new CompletenessCheck
            {
                CheckID = id,
                MissingItemsJSON = dto.MissingItemsJSON,
                Status = dto.Status,
                CheckedDate = dto.CheckedDate
            };

            var updated = await _repository.UpdateAsync(check);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<CompletenessCheckResponseDto?> UpdateCheckStatusAsync(Guid id, UpdateCheckStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            return updated is null ? null : MapToResponseDto(updated);
        }

        private static CompletenessCheckResponseDto MapToResponseDto(CompletenessCheck check) => new()
        {
            CheckID = check.CheckID,
            SubmissionID = check.SubmissionID,
            MissingItemsJSON = check.MissingItemsJSON,
            Status = check.Status,
            CheckedDate = check.CheckedDate
        };
    }
}
