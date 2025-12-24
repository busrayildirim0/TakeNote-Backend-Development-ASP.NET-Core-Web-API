namespace TakeNote.Core.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public List<string> Tags { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        // ATTACHMENT KALDIRILDI ❌
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public int Version { get; set; }
    }
}