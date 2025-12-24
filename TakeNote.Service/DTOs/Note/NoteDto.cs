// TakeNote.Service/DTOs/Note/NoteDto.cs - GÜNCELLENMİŞ
namespace TakeNote.Service.DTOs
{
    public class NoteDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public List<string> Tags { get; set; } = new(); // YENİ
        public int? WorkspaceId { get; set; }
        public Guid CreatedById { get; set; }
        public string? CreatedByUsername { get; set; } // YENİ: Frontend için
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // YENİ: Görevler
        public List<TaskItemDto> Tasks { get; set; } = new();
    }
}