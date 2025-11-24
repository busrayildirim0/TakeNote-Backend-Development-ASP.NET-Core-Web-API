namespace TakeNote.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime? DueDate { get; set; }

        // Foreign Keys
        public int NoteId { get; set; }
        public Note Note { get; set; } = null!;

        public Guid? AssignedToId { get; set; } // Kime atandı? (Opsiyonel)
        public User? AssignedTo { get; set; }
    }
}