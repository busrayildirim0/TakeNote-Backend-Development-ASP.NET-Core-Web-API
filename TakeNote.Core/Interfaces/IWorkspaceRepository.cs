using TakeNote.Core.Entities;

namespace TakeNote.Core.Interfaces
{
    public interface IWorkspaceRepository : IRepository<Workspace>
    {
        // Bu metodu ekliyoruz:
        Task<Workspace?> GetByIdWithMembersAsync(int id);
    }
}