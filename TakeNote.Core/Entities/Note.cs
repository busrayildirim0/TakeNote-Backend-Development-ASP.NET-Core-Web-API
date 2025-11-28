namespace TakeNote.Core.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public Guid CreatedById { get; set; } // Notu oluşturan kullanıcı
        public User CreatedBy { get; set; } = null!;

        // İlişkiler
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        // Versiyon kontrolü (Concurrency) için
        public int Version { get; set; }
    }
}