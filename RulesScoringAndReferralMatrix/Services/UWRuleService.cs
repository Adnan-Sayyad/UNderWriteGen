using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class UWRuleService : IUWRuleService
    {
        private readonly IUWRuleRepository       _repository;
        private readonly IRiskScoreService       _riskScoreService;
        private readonly INotificationClientService _notifications;
        private readonly ISubmissionClientService   _submissionClient;

        public UWRuleService(
            IUWRuleRepository           repository,
            IRiskScoreService           riskScoreService,
            INotificationClientService  notifications,
            ISubmissionClientService    submissionClient)
        {
            _repository       = repository;
            _riskScoreService = riskScoreService;
            _notifications    = notifications;
            _submissionClient = submissionClient;
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

        /// <summary>
        /// Evaluates all active UW rules against actual submission data.
        ///
        /// Flow:
        ///   1. Fetch submission from Submission service.
        ///   2. Calculate / refresh the risk score based on real factors.
        ///   3. Evaluate each rule's ExpressionJSON against the submission.
        ///   4. If any Block rule triggers  → escalate band to Unacceptable (score = 100).
        ///   5. If any Refer rule triggers  → send referral notification.
        ///   6. Load rules that trigger are surfaced in results for Pricing to act on.
        /// </summary>
        public async Task<EvaluateRulesResponseDto> EvaluateRulesForSubmissionAsync(Guid submissionId)
        {
            // Fetch real submission data to evaluate rule conditions against
            var submission = await _submissionClient.GetSubmissionAsync(submissionId)
                ?? throw new KeyNotFoundException($"Submission '{submissionId}' not found.");

            // Calculate risk score from actual submission factors
            var riskScore = await _riskScoreService.CalculateScoreForSubmissionAsync(submissionId);

            var activeRules = await _repository.GetActiveRulesAsync();

            var results = new List<RuleEvaluationResultDto>();
            bool hasBlockRule = false;
            bool hasReferRule = false;

            foreach (var rule in activeRules)
            {
                // Evaluate the rule's ExpressionJSON against the real submission data
                bool triggered = ExpressionEvaluator.Evaluate(rule.ExpressionJSON, submission);

                if (triggered)
                {
                    if (rule.Severity == Severity.Block)
                        hasBlockRule = true;
                    if (rule.Severity == Severity.Refer)
                        hasReferRule = true;
                }

                results.Add(new RuleEvaluationResultDto
                {
                    UWRuleID    = rule.UWRuleID,
                    RuleName    = rule.RuleName,
                    ProductLine = rule.ProductLine,
                    Severity    = rule.Severity,
                    Triggered   = triggered,
                    Message     = triggered
                        ? $"Rule '{rule.RuleName ?? rule.UWRuleID.ToString()}' TRIGGERED " +
                          $"(Severity={rule.Severity}) for submission {submissionId}."
                        : $"Rule '{rule.RuleName ?? rule.UWRuleID.ToString()}' did not trigger."
                });
            }

            // A Block rule means the risk is unacceptable — escalate band regardless of base score
            if (hasBlockRule)
            {
                riskScore = await _riskScoreService.UpsertScoreAsync(
                    submissionId, 100.0, Band.Unacceptable, "v2.0");

                _ = _notifications.BroadcastAsync(
                    "Underwriter",
                    $"Submission {submissionId} has triggered a BLOCK rule and is marked Unacceptable.",
                    "Compliance");
            }
            else if (hasReferRule)
            {
                _ = _notifications.BroadcastAsync(
                    "UWManager",
                    $"Submission {submissionId} has triggered a REFER rule and requires senior review.",
                    "Referral");
            }

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
