namespace TakeNote.Service.DTOs
{
    public class TaskItemUpdateDto
    {
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}