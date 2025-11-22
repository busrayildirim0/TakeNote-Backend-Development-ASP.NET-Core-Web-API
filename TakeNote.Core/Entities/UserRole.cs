namespace TakeNote.Core.Entities
{
    public class UserRole
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!; // Null uyarısını susturuyoruz
    }
}