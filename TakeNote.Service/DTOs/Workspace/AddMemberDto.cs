using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class AddMemberDto
    {
        // GUID (Opsiyonel - eski sistem için)
        public Guid UserId { get; set; } = Guid.Empty;

        // Username VEYA Email
        public string? UserIdentifier { get; set; }

        [Required]
        public string Role { get; set; } = "Viewer";
    }
}