using Microsoft.EntityFrameworkCore;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;

namespace TakeNote.DataAccess.Repositories
{
    public class NoteRepository : EfRepository<Note>, INoteRepository
    {
        public NoteRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Note?> GetByIdWithRelationsAsync(int id)
        {
            return await _context.Notes
                // .Include(n => n.Attachments) ❌ BU SATIR SİLİNDİ
                .Include(n => n.Tasks)
                .Include(n => n.CreatedBy)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Note>> GetNotesByWorkspaceAsync(int workspaceId)
        {
            return await _context.Notes
                .Where(n => n.WorkspaceId == workspaceId)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }
    }
}