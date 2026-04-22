using Microsoft.EntityFrameworkCore;
using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class UWNoteService : IUWNoteService
    {
        private readonly UWWorkflowDbContext _db;

        public UWNoteService(UWWorkflowDbContext db) => _db = db;

        public IEnumerable<UWNote> GetBySubmission(Guid submissionId) =>
            _db.UWNotes.Where(n => n.SubmissionID == submissionId).ToList();

        public UWNote? GetById(Guid noteId) =>
            _db.UWNotes.FirstOrDefault(n => n.NoteID == noteId);

        public IEnumerable<UWNote> GetByAuthor(Guid authorId) =>
            _db.UWNotes.Where(n => n.AuthorID == authorId).ToList();

        public UWNote Add(CreateUWNoteDto dto)
        {
            var note = new UWNote
            {
                SubmissionID = dto.SubmissionID,
                AuthorID = dto.AuthorID,
                NoteText = dto.NoteText
            };
            _db.UWNotes.Add(note);
            _db.SaveChanges();
            return note;
        }

        public UWNote? Update(Guid noteId, UpdateUWNoteDto dto)
        {
            var note = _db.UWNotes.FirstOrDefault(n => n.NoteID == noteId);
            if (note is null) return null;
            note.NoteText = dto.NoteText;
            _db.SaveChanges();
            return note;
        }

        public bool Delete(Guid noteId)
        {
            var note = _db.UWNotes.FirstOrDefault(n => n.NoteID == noteId);
            if (note is null) return false;
            _db.UWNotes.Remove(note);
            _db.SaveChanges();
            return true;
        }
    }
}
