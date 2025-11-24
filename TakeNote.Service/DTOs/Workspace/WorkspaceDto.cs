namespace TakeNote.Service.DTOs
{
    public class WorkspaceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid OwnerId { get; set; }
    }
}