// TakeNote.Core/Entities/TaskItem.cs
namespace TakeNote.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        // YENİ: Description yerine Title (Frontend'deki gibi)
        public string Description { get; set; } = string.Empty; // Opsiyonel detay

        public bool IsCompleted { get; set; } = false;
        public DateTime? DueDate { get; set; }

        // Foreign Keys
        public int NoteId { get; set; }
        public Note Note { get; set; } = null!;

        public Guid? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }
    }
}