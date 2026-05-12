using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class SubjectivityService : ISubjectivityService
    {
        private readonly UWWorkflowDbContext _db;
        private readonly INotificationClientService _notifications;

        public SubjectivityService(UWWorkflowDbContext db, INotificationClientService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public IEnumerable<Subjectivity> GetBySubmission(Guid submissionId) =>
            _db.Subjectivities.Where(s => s.SubmissionID == submissionId).ToList();

        public Subjectivity? GetById(Guid subjectivityId) =>
            _db.Subjectivities.FirstOrDefault(s => s.SubjectivityID == subjectivityId);

        public IEnumerable<Subjectivity> GetDueBy(DateTime date) =>
            _db.Subjectivities.Where(s => s.DueDate.Date <= date.Date).ToList();

        public Subjectivity Add(CreateSubjectivityDto dto)
        {
            var subjectivity = new Subjectivity
            {
                SubmissionID = dto.SubmissionID,
                Description = dto.Description,
                DueDate = dto.DueDate
            };
            _db.Subjectivities.Add(subjectivity);
            _db.SaveChanges();

            // PDF §2.11/§4.11: subjectivity due — let the agent and UW assistant know.
            var message = $"New subjectivity for submission '{dto.SubmissionID}' due by {dto.DueDate:yyyy-MM-dd}. {dto.Description}";
            _ = _notifications.BroadcastAsync("Agent",       message, "Subjectivity");
            _ = _notifications.BroadcastAsync("UWAssistant", message, "Subjectivity");

            return subjectivity;
        }

        public Subjectivity? Update(Guid subjectivityId, UpdateSubjectivityDto dto)
        {
            var s = _db.Subjectivities.FirstOrDefault(x => x.SubjectivityID == subjectivityId);
            if (s is null) return null;
            s.Description = dto.Description;
            s.DueDate = dto.DueDate;
            _db.SaveChanges();
            return s;
        }

        public Subjectivity? UpdateStatus(Guid subjectivityId, UpdateSubjectivityStatusDto dto)
        {
            var s = _db.Subjectivities.FirstOrDefault(x => x.SubjectivityID == subjectivityId);
            if (s is null) return null;
            s.Status = dto.Status;
            _db.SaveChanges();

            // Met / Waived → notify underwriters so they can progress the submission.
            if (dto.Status is "Met" or "Waived")
            {
                var message = $"Subjectivity '{s.SubjectivityID}' marked as {dto.Status} for submission '{s.SubmissionID}'.";
                _ = _notifications.BroadcastAsync("Underwriter", message, "Subjectivity");
            }

            return s;
        }

        public bool Delete(Guid subjectivityId)
        {
            var s = _db.Subjectivities.FirstOrDefault(x => x.SubjectivityID == subjectivityId);
            if (s is null) return false;
            _db.Subjectivities.Remove(s);
            _db.SaveChanges();
            return true;
        }
    }
}
