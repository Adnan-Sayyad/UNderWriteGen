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

        public UWRuleService(IUWRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UWRuleResponseDto>> GetAllRulesAsync()
        {
            var rules = await _repository.GetAllAsync();
            return rules.Select(MapToResponseDto);
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
                ProductLine = dto.ProductLine,
                ExpressionJSON = dto.ExpressionJSON,
                Severity = dto.Severity,
                Status = dto.Status
            };

            var created = await _repository.CreateAsync(rule);
            return MapToResponseDto(created);
        }

        public async Task<UWRuleResponseDto?> UpdateRuleAsync(Guid id, UpdateUWRuleDto dto)
        {
            var rule = new UWRule
            {
                UWRuleID = id,
                ProductLine = dto.ProductLine,
                ExpressionJSON = dto.ExpressionJSON,
                Severity = dto.Severity,
                Status = dto.Status
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
            var activeRules = await _repository.GetActiveRulesAsync();
            var results = activeRules.Select(rule => new RuleEvaluationResultDto
            {
                UWRuleID = rule.UWRuleID,
                ProductLine = rule.ProductLine,
                Severity = rule.Severity,
                Triggered = true,
                Message = $"Rule {rule.UWRuleID} evaluated for submission {submissionId}"
            }).ToList();

            return new EvaluateRulesResponseDto
            {
                SubmissionID = submissionId,
                Results = results,
                HasBlockingRules = results.Any(r => r.Triggered && r.Severity == Severity.Block),
                EvaluatedAt = DateTime.UtcNow
            };
        }

        private static UWRuleResponseDto MapToResponseDto(UWRule rule) => new()
        {
            UWRuleID = rule.UWRuleID,
            ProductLine = rule.ProductLine,
            ExpressionJSON = rule.ExpressionJSON,
            Severity = rule.Severity,
            Status = rule.Status
        };
    }
}
