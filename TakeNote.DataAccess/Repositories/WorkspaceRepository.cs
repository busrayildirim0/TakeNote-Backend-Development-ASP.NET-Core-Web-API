using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;

namespace TakeNote.DataAccess.Repositories
{
    public class WorkspaceRepository : EfRepository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(AppDbContext context) : base(context)
        {
        }
    }
}