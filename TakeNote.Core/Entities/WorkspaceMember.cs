namespace TakeNote.Core.Entities
{
    public class WorkspaceMember
    {
        public Guid UserId { get; set; }
        // "= null!;" diyerek derleyiciye "Merak etme, EF Core bunu dolduracak" diyoruz.
        public User User { get; set; } = null!;

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string Role { get; set; } = "Viewer";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}