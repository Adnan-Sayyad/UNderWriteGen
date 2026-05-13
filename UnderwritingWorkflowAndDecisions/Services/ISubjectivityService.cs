using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface ISubjectivityService
    {
        IEnumerable<Subjectivity> GetAll();
        IEnumerable<Subjectivity> GetBySubmission(Guid submissionId);
        Subjectivity? GetById(Guid subjectivityId);
        IEnumerable<Subjectivity> GetDueBy(DateTime date);
        Subjectivity Add(CreateSubjectivityDto dto);
        Subjectivity? Update(Guid subjectivityId, UpdateSubjectivityDto dto);
        Subjectivity? UpdateStatus(Guid subjectivityId, UpdateSubjectivityStatusDto dto);
        bool Delete(Guid subjectivityId);
    }
}
