using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class NoteCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [Required]
        public int WorkspaceId { get; set; } // Not hangi çalışma alanında?

        public bool IsPinned { get; set; } = false;
    }
}