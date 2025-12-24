using TakeNote.Core.Entities;

namespace TakeNote.Core.Interfaces
{
    public interface IWorkspaceRepository : IRepository<Workspace>
    {
        Task<Workspace?> GetByIdWithMembersAsync(int id);
    }
}