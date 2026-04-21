using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Data;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Repositories
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly SubmissionAndIntakeDbContext _context;

        public QuestionnaireRepository(SubmissionAndIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Questionnaire>> GetAllAsync()
        {
            return await _context.Questionnaires.ToListAsync();
        }

        public async Task<Questionnaire?> GetByIdAsync(Guid id)
        {
            return await _context.Questionnaires.FirstOrDefaultAsync(q => q.QID == id);
        }

        public async Task<IEnumerable<Questionnaire>> GetBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.Questionnaires
                .Where(q => q.SubmissionID == submissionId)
                .ToListAsync();
        }

        public async Task<Questionnaire> CreateAsync(Questionnaire questionnaire)
        {
            questionnaire.QID = Guid.NewGuid();
            _context.Questionnaires.Add(questionnaire);
            await _context.SaveChangesAsync();
            return questionnaire;
        }

        public async Task<Questionnaire?> UpdateAsync(Questionnaire questionnaire)
        {
            var existing = await _context.Questionnaires.FirstOrDefaultAsync(q => q.QID == questionnaire.QID);
            if (existing is null) return null;

            existing.TemplateVersion = questionnaire.TemplateVersion;
            existing.ResponsesJSON = questionnaire.ResponsesJSON;
            existing.CompletedDate = questionnaire.CompletedDate;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var questionnaire = await _context.Questionnaires.FirstOrDefaultAsync(q => q.QID == id);
            if (questionnaire is null) return false;

            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
