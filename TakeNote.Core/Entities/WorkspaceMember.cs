namespace TakeNote.Core.Entities
{
    public class WorkspaceMember
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string Role { get; set; } = "Viewer";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}