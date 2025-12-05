using Microsoft.EntityFrameworkCore;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;

namespace TakeNote.DataAccess.Repositories
{
    public class WorkspaceRepository : EfRepository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(AppDbContext context) : base(context)
        {
        }

        // YENİ EKLENEN METOT
        public async Task<Workspace?> GetByIdWithMembersAsync(int id)
        {
            return await _context.Workspaces
                .Include(w => w.Members)       // Üyeleri getir
                .ThenInclude(m => m.User)      // (Opsiyonel) Üyelerin kullanıcı detaylarını da getir (Ad, Email vb.)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}