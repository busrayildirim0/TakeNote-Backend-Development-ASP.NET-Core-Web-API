namespace TakeNote.Service.DTOs
{
    public class UserAuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // JWT Access Token
        public string RefreshToken { get; set; } = string.Empty;
    }
}