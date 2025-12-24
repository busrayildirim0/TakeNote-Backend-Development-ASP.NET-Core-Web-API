// TakeNote.Service/DTOs/Note/NoteUpdateDto.cs - GÜNCELLENMİŞ
namespace TakeNote.Service.DTOs
{
    public class NoteUpdateDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsLocked { get; set; }
        public List<string>? Tags { get; set; } // YENİ
        public List<TaskItemDto>? Tasks { get; set; } // YENİ
    }
}