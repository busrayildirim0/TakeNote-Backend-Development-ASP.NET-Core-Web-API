using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class NoteCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;


        public int? WorkspaceId { get; set; }
        public bool IsPinned { get; set; } = false;
    }
}