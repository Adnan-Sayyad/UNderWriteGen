using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class RiskScoreService : IRiskScoreService
    {
        private readonly IRiskScoreRepository      _repository;
        private readonly ISubmissionClientService  _submissionClient;

        public RiskScoreService(
            IRiskScoreRepository     repository,
            ISubmissionClientService submissionClient)
        {
            _repository       = repository;
            _submissionClient = submissionClient;
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
                ScoreValue   = dto.ScoreValue,
                Band         = dto.Band,
                ScoredDate   = dto.ScoredDate == default ? DateTime.UtcNow : dto.ScoredDate
            };
            var created = await _repository.CreateAsync(score);
            return MapToResponseDto(created);
        }

        public async Task<RiskScoreResponseDto> UpsertScoreAsync(
            Guid submissionId, double scoreValue, Band band, string modelVersion)
        {
            var record = await _repository.UpsertScoreAsync(submissionId, scoreValue, band, modelVersion);
            return MapToResponseDto(record);
        }

        /// <summary>
        /// Calculates a risk score based on real submission factors fetched from the
        /// Submission service. Factors: occupation type, sum insured, product line, tenure.
        /// Score range: 0–100  (higher = more risky)
        /// Bands: Low &lt; 35 | Medium 35–64 | High 65–84 | Unacceptable ≥ 85
        /// </summary>
        public async Task<RiskScoreResponseDto> CalculateScoreForSubmissionAsync(Guid submissionId)
        {
            var submission = await _submissionClient.GetSubmissionAsync(submissionId)
                ?? throw new KeyNotFoundException($"Submission '{submissionId}' not found.");

            double score = 20.0; // base score

            // ── Occupation factor ────────────────────────────────────────
            string occ = submission.OccupationType.ToLowerInvariant();
            score += occ switch
            {
                var o when o.Contains("explo")   || o.Contains("demoli")   => 45,
                var o when o.Contains("chemi")   || o.Contains("nuclear")  => 40,
                var o when o.Contains("mining")                             => 40,
                var o when o.Contains("construct")                          => 30,
                var o when o.Contains("offshore") || o.Contains("fishing") => 25,
                var o when o.Contains("manufactur")                         => 20,
                var o when o.Contains("transport") || o.Contains("logist") => 15,
                var o when o.Contains("health") || o.Contains("medical")   => 15,
                var o when o.Contains("retail") || o.Contains("hospit")    => 10,
                var o when o.Contains("office") || o.Contains("admin")
                        || o.Contains("clerk") || o.Contains("profess")
                        || o.Contains("it")    || o.Contains("software")
                        || o.Contains("legal") || o.Contains("finance")
                        || o.Contains("educat")                             => 0,
                _ => 10  // unknown occupations treated as moderate
            };

            // ── Sum insured factor ───────────────────────────────────────
            score += submission.SumInsured switch
            {
                > 100_000_000m => 30,
                > 50_000_000m  => 25,
                > 20_000_000m  => 15,
                > 10_000_000m  => 10,
                > 5_000_000m   => 5,
                _              => 0
            };

            // ── Product line factor ──────────────────────────────────────
            string pl = submission.ProductLine.ToLowerInvariant();
            score += pl switch
            {
                "commercial" => 10,
                "pnc"        => 5,
                "health"     => 5,
                _            => 0  // Life
            };

            // ── Tenure factor ────────────────────────────────────────────
            score += submission.PolicyTenureMonths switch
            {
                >= 60 => 5,
                >= 36 => 3,
                < 6   => -5,
                _     => 0
            };

            score = Math.Round(Math.Clamp(score, 0.0, 100.0), 2);

            var band = score switch
            {
                >= 85 => Band.Unacceptable,
                >= 65 => Band.High,
                >= 35 => Band.Medium,
                _     => Band.Low
            };

            var record = await _repository.UpsertScoreAsync(submissionId, score, band, "v2.0");
            return MapToResponseDto(record);
        }

        private static RiskScoreResponseDto MapToResponseDto(RiskScore score) => new()
        {
            RiskScoreID  = score.RiskScoreID,
            SubmissionID = score.SubmissionID,
            ModelVersion = score.ModelVersion,
            ScoreValue   = score.ScoreValue,
            Band         = score.Band,
            ScoredDate   = score.ScoredDate
        };
    }
}
