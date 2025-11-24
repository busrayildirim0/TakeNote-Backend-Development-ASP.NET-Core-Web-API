using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class WorkspaceCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = true;
    }
}