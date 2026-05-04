using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Contracts.ServiceContracts
{
    public interface ISubmissionService
    {
        Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync();
        Task<SubmissionResponseDto?> GetSubmissionByIdAsync(Guid id);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAgentIdAsync(string agentId);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByPartyIdAsync(string partyId);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByStatusAsync(SubmissionStatus status);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByProductLineAsync(ProductLine productLine);
        Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto);
        Task<SubmissionResponseDto?> UpdateSubmissionAsync(Guid id, UpdateSubmissionDto dto);
        Task<SubmissionResponseDto?> UpdateSubmissionStatusAsync(Guid id, UpdateSubmissionStatusDto dto);
        Task<bool> DeleteSubmissionAsync(Guid id);
    }
}
