namespace TakeNote.Service.DTOs
{
    public class UserUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        // Şifre güncelleme ayrı bir metot olmalı, buraya koymuyoruz.
    }
}