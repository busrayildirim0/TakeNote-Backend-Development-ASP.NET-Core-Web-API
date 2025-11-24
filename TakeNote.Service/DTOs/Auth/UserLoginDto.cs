using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class UserLoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty; // Veya Username

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}