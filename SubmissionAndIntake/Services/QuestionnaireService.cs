using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _repository;

        public QuestionnaireService(IQuestionnaireRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<QuestionnaireResponseDto>> GetAllQuestionnairesAsync()
        {
            var questionnaires = await _repository.GetAllAsync();
            return questionnaires.Select(MapToResponseDto);
        }

        public async Task<QuestionnaireResponseDto?> GetQuestionnaireByIdAsync(Guid id)
        {
            var questionnaire = await _repository.GetByIdAsync(id);
            return questionnaire is null ? null : MapToResponseDto(questionnaire);
        }

        public async Task<IEnumerable<QuestionnaireResponseDto>> GetQuestionnairesBySubmissionIdAsync(Guid submissionId)
        {
            var questionnaires = await _repository.GetBySubmissionIdAsync(submissionId);
            return questionnaires.Select(MapToResponseDto);
        }

        public async Task<QuestionnaireResponseDto> CreateQuestionnaireAsync(CreateQuestionnaireDto dto)
        {
            var questionnaire = new Questionnaire
            {
                SubmissionID = dto.SubmissionID,
                TemplateVersion = dto.TemplateVersion,
                ResponsesJSON = dto.ResponsesJSON,
                CompletedDate = dto.CompletedDate
            };

            var created = await _repository.CreateAsync(questionnaire);
            return MapToResponseDto(created);
        }

        public async Task<QuestionnaireResponseDto?> UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireDto dto)
        {
            var questionnaire = new Questionnaire
            {
                QID = id,
                TemplateVersion = dto.TemplateVersion,
                ResponsesJSON = dto.ResponsesJSON,
                CompletedDate = dto.CompletedDate
            };

            var updated = await _repository.UpdateAsync(questionnaire);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<QuestionnaireResponseDto?> UpdateQuestionnaireStatusAsync(Guid id, UpdateQuestionnaireStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteQuestionnaireAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<QuestionnaireTemplateDto>> GetTemplatesAsync()
        {
            var versions = await _repository.GetDistinctTemplateVersionsAsync();
            return versions.Select(v => new QuestionnaireTemplateDto
            {
                Version = v,
                Description = $"Questionnaire template version {v}"
            });
        }

        public async Task<QuestionnaireTemplateDto?> GetTemplateByVersionAsync(string version)
        {
            var versions = await _repository.GetDistinctTemplateVersionsAsync();
            if (!versions.Contains(version)) return null;
            return new QuestionnaireTemplateDto
            {
                Version = version,
                Description = $"Questionnaire template version {version}"
            };
        }

        private static QuestionnaireResponseDto MapToResponseDto(Questionnaire questionnaire) => new()
        {
            QID = questionnaire.QID,
            SubmissionID = questionnaire.SubmissionID,
            TemplateVersion = questionnaire.TemplateVersion,
            ResponsesJSON = questionnaire.ResponsesJSON,
            CompletedDate = questionnaire.CompletedDate,
            Status = questionnaire.Status
        };
    }
}
