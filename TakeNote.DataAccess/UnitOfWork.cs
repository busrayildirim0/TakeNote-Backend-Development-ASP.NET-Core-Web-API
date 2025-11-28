using TakeNote.Core.Interfaces;
using TakeNote.DataAccess.Repositories;

namespace TakeNote.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // Repository'leri sadece ihtiyaç duyulduğunda oluşturmak için (Lazy Loading) private field tutuyoruz
        private IUserRepository? _userRepository;
        private IWorkspaceRepository? _workspaceRepository;
        private INoteRepository? _noteRepository;
        private ITaskItemRepository? _taskItemRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Repository İstendiğinde: Varsa eskisini ver, yoksa yenisini oluştur.
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IWorkspaceRepository Workspaces => _workspaceRepository ??= new WorkspaceRepository(_context);
        public INoteRepository Notes => _noteRepository ??= new NoteRepository(_context);
        public ITaskItemRepository TaskItems => _taskItemRepository ??= new TaskItemRepository(_context);

        // Değişiklikleri Kaydet
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}