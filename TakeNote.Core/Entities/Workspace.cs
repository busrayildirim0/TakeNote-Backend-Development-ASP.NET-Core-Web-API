using System.Collections.Generic;

namespace TakeNote.Core.Entities
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid OwnerId { get; set; }
        public bool IsPrivate { get; set; }

        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}