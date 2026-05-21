using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class UWRuleService : IUWRuleService
    {
        private readonly IUWRuleRepository _repository;
        private readonly IRiskScoreService _riskScoreService;
        private readonly INotificationClientService _notifications;

        public UWRuleService(
            IUWRuleRepository repository,
            IRiskScoreService riskScoreService,
            INotificationClientService notifications)
        {
            _repository = repository;
            _riskScoreService = riskScoreService;
            _notifications = notifications;
        }

        public async Task<IEnumerable<UWRuleResponseDto>> GetAllRulesAsync()
        {
            var rules = await _repository.GetAllAsync();
            return rules.Select(MapToResponseDto);
        }

        public async Task<PagedResultDto<UWRuleResponseDto>> GetRulesPagedAsync(
            int page, int size,
            string? productLine, Severity? severity, UWStatus? status)
        {
            var (items, total) = await _repository.GetPagedAsync(page, size, productLine, severity, status);
            return new PagedResultDto<UWRuleResponseDto>
            {
                Content       = items.Select(MapToResponseDto),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = size > 0 ? (int)Math.Ceiling(total / (double)size) : 0,
            };
        }

        public async Task<UWRuleResponseDto?> GetRuleByIdAsync(Guid id)
        {
            var rule = await _repository.GetByIdAsync(id);
            return rule is null ? null : MapToResponseDto(rule);
        }

        public async Task<IEnumerable<UWRuleResponseDto>> GetRulesByProductLineAsync(string productLine)
        {
            var rules = await _repository.GetByProductLineAsync(productLine);
            return rules.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UWRuleResponseDto>> GetActiveRulesAsync()
        {
            var rules = await _repository.GetActiveRulesAsync();
            return rules.Select(MapToResponseDto);
        }

        public async Task<UWRuleResponseDto> CreateRuleAsync(CreateUWRuleDto dto)
        {
            var rule = new UWRule
            {
                ProductLine    = dto.ProductLine,
                RuleName       = dto.RuleName,
                Description    = dto.Description,
                ExpressionJSON = dto.ExpressionJSON,
                Severity       = dto.Severity,
                Status         = dto.Status
            };
            var created = await _repository.CreateAsync(rule);
            return MapToResponseDto(created);
        }

        public async Task<UWRuleResponseDto?> UpdateRuleAsync(Guid id, UpdateUWRuleDto dto)
        {
            var rule = new UWRule
            {
                UWRuleID       = id,
                ProductLine    = dto.ProductLine,
                RuleName       = dto.RuleName,
                Description    = dto.Description,
                ExpressionJSON = dto.ExpressionJSON,
                Severity       = dto.Severity,
                Status         = dto.Status
            };
            var updated = await _repository.UpdateAsync(rule);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<IEnumerable<UWRuleResponseDto>> GetRulesBySeverityAsync(Severity severity)
        {
            var rules = await _repository.GetBySeverityAsync(severity);
            return rules.Select(MapToResponseDto);
        }

        public async Task<UWRuleResponseDto?> UpdateRuleStatusAsync(Guid id, UpdateRuleStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteRuleAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<EvaluateRulesResponseDto> EvaluateRulesForSubmissionAsync(Guid submissionId)
        {
            var riskScore   = await _riskScoreService.CalculateScoreForSubmissionAsync(submissionId);
            var activeRules = await _repository.GetActiveRulesAsync();

            var results = activeRules.Select(rule => new RuleEvaluationResultDto
            {
                UWRuleID    = rule.UWRuleID,
                RuleName    = rule.RuleName,
                ProductLine = rule.ProductLine,
                Severity    = rule.Severity,
                Triggered   = true,
                Message     = $"Rule '{rule.RuleName ?? rule.UWRuleID.ToString()}' evaluated for submission {submissionId}"
            }).ToList();

            return new EvaluateRulesResponseDto
            {
                SubmissionID = submissionId,
                Results      = results,
                RiskScore    = riskScore.ScoreValue,
                RiskBand     = riskScore.Band.ToString(),
                EvaluatedAt  = DateTime.UtcNow
            };
        }

        private static UWRuleResponseDto MapToResponseDto(UWRule rule) => new()
        {
            UWRuleID       = rule.UWRuleID,
            ProductLine    = rule.ProductLine,
            RuleName       = rule.RuleName,
            Description    = rule.Description,
            ExpressionJSON = rule.ExpressionJSON,
            Severity       = rule.Severity,
            Status         = rule.Status
        };
    }
}
