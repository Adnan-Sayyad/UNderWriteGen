using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface IUWNoteService
    {
        IEnumerable<UWNote> GetBySubmission(Guid submissionId);
        UWNote? GetById(Guid noteId);
        IEnumerable<UWNote> GetByAuthor(Guid authorId);
        UWNote Add(CreateUWNoteDto dto);
        UWNote? Update(Guid noteId, UpdateUWNoteDto dto);
        bool Delete(Guid noteId);
    }
}
