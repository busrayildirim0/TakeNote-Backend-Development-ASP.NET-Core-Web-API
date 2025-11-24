using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class AddMemberDto
    {
        [Required]
        public Guid UserId { get; set; } // Eklenecek kişinin ID'si

        public string Role { get; set; } = "Viewer"; // Admin, Editor, Viewer
    }
}
