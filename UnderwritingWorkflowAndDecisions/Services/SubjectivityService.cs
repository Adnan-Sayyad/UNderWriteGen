using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class SubjectivityService : ISubjectivityService
    {
        private readonly UWWorkflowDbContext _db;

        public SubjectivityService(UWWorkflowDbContext db) => _db = db;

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
