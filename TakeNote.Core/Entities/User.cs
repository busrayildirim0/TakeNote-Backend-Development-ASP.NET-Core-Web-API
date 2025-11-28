using System.Collections.Generic; // Bunu eklemeyi unutma

namespace TakeNote.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<WorkspaceMember> Workspaces { get; set; } = new List<WorkspaceMember>();
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}