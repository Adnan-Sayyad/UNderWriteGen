using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class RiskScoreService : IRiskScoreService
    {
        private readonly IRiskScoreRepository _repository;

        public RiskScoreService(IRiskScoreRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RiskScoreResponseDto>> GetAllScoresAsync()
        {
            var scores = await _repository.GetAllAsync();
            return scores.Select(MapToResponseDto);
        }

        public async Task<RiskScoreResponseDto?> GetScoreByIdAsync(Guid id)
        {
            var score = await _repository.GetByIdAsync(id);
            return score is null ? null : MapToResponseDto(score);
        }

        public async Task<IEnumerable<RiskScoreResponseDto>> GetScoresBySubmissionIdAsync(Guid submissionId)
        {
            var scores = await _repository.GetBySubmissionIdAsync(submissionId);
            return scores.Select(MapToResponseDto);
        }

        public async Task<RiskScoreResponseDto> CreateScoreAsync(CreateRiskScoreDto dto)
        {
            var score = new RiskScore
            {
                SubmissionID = dto.SubmissionID,
                ModelVersion = dto.ModelVersion,
                ScoreValue = dto.ScoreValue,
                Band = dto.Band,
                ScoredDate = dto.ScoredDate == default ? DateTime.UtcNow : dto.ScoredDate
            };

            var created = await _repository.CreateAsync(score);
            return MapToResponseDto(created);
        }

        private static RiskScoreResponseDto MapToResponseDto(RiskScore score) => new()
        {
            RiskScoreID = score.RiskScoreID,
            SubmissionID = score.SubmissionID,
            ModelVersion = score.ModelVersion,
            ScoreValue = score.ScoreValue,
            Band = score.Band,
            ScoredDate = score.ScoredDate
        };
    }
}
