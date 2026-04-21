using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Contracts.ServiceContracts
{
    public interface ICompletenessCheckService
    {
        Task<IEnumerable<CompletenessCheckResponseDto>> GetAllChecksAsync();
        Task<CompletenessCheckResponseDto?> GetCheckByIdAsync(Guid id);
        Task<IEnumerable<CompletenessCheckResponseDto>> GetChecksBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<CompletenessCheckResponseDto>> GetChecksByStatusAsync(CheckStatus status);
        Task<CompletenessCheckResponseDto> CreateCheckAsync(CreateCompletenessCheckDto dto);
        Task<CompletenessCheckResponseDto?> UpdateCheckAsync(Guid id, UpdateCompletenessCheckDto dto);
    }
}
