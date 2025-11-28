using TakeNote.Core.Entities;

namespace TakeNote.Core.Interfaces
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<Note?> GetByIdWithRelationsAsync(int id);
        
        Task<IEnumerable<Note>> GetNotesByWorkspaceAsync(int workspaceId);
    }
}