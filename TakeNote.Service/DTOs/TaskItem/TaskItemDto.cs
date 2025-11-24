namespace TakeNote.Service.DTOs
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}