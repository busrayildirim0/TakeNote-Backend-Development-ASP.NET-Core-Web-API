using TakeNote.Core.Entities;

namespace TakeNote.Core.Interfaces
{
    public interface INoteRepository : IRepository<Note>
    {
        // Notlara özel metotlar (UML'deki gibi)
        Task<Note?> GetByIdWithRelationsAsync(int id);
        // PagedResult yapısını Service katmanında kurmuştuk ama Repo'da query dönebiliriz
        // veya direkt list dönebiliriz. Şimdilik list:
        Task<IEnumerable<Note>> GetNotesByWorkspaceAsync(int workspaceId);
    }
}