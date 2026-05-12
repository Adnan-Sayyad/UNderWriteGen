using RulesScoringAndReferralMatrix.configs.Enums;
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

        public async Task<PagedResultDto<RiskScoreResponseDto>> GetScoresBySubmissionPagedAsync(
            Guid submissionId, int page, int size)
        {
            var (items, total) = await _repository.GetPagedBySubmissionIdAsync(submissionId, page, size);
            return new PagedResultDto<RiskScoreResponseDto>
            {
                Content       = items.Select(MapToResponseDto),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = size > 0 ? (int)Math.Ceiling(total / (double)size) : 0,
            };
        }

        public async Task<RiskScoreResponseDto?> GetLatestScoreBySubmissionIdAsync(Guid submissionId)
        {
            var score = await _repository.GetLatestBySubmissionIdAsync(submissionId);
            return score is null ? null : MapToResponseDto(score);
        }

        public async Task<IEnumerable<RiskScoreResponseDto>> GetScoresByBandAsync(Band band)
        {
            var scores = await _repository.GetByBandAsync(band);
            return scores.Select(MapToResponseDto);
        }

        public async Task<PagedResultDto<RiskScoreResponseDto>> GetScoresByBandPagedAsync(
            Band band, int page, int size)
        {
            var (items, total) = await _repository.GetPagedByBandAsync(band, page, size);
            return new PagedResultDto<RiskScoreResponseDto>
            {
                Content       = items.Select(MapToResponseDto),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = size > 0 ? (int)Math.Ceiling(total / (double)size) : 0,
            };
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

        public async Task<RiskScoreResponseDto> CalculateScoreForSubmissionAsync(Guid submissionId)
        {
            var scoreValue = Math.Round(new Random().NextDouble() * 100, 2);
            var band = scoreValue < 33 ? Band.Low : scoreValue < 66 ? Band.Medium : Band.High;

            var score = new RiskScore
            {
                SubmissionID = submissionId,
                ModelVersion = "v1.0",
                ScoreValue = scoreValue,
                Band = band,
                ScoredDate = DateTime.UtcNow
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
