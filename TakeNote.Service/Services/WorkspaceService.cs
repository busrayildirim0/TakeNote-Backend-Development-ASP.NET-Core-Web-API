using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces; // IUnitOfWork burada
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces; // IWorkspaceService burada

namespace TakeNote.Service.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IUnitOfWork _unitOfWork; // DEĞİŞİKLİK 1: Repository yerine UnitOfWork

        public WorkspaceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId)
        {
            var workspace = new Workspace
            {
                Title = dto.Title,
                Description = dto.Description,
                IsPrivate = dto.IsPrivate,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                Members = new List<WorkspaceMember>
                {
                    // Oluşturan kişi Admin olur
                    new WorkspaceMember { UserId = userId, Role = "Admin", JoinedAt = DateTime.UtcNow }
                }
            };

            // DEĞİŞİKLİK 2: UnitOfWork üzerinden ekleme
            await _unitOfWork.Workspaces.AddAsync(workspace);

            // DEĞİŞİKLİK 3: KAYDETME (Bu olmazsa veritabanına gitmez!)
            await _unitOfWork.CompleteAsync();

            return new WorkspaceDto
            {
                Id = workspace.Id,
                Title = workspace.Title,
                Description = workspace.Description,
                OwnerId = workspace.OwnerId,
                IsPrivate = workspace.IsPrivate,
                CreatedAt = workspace.CreatedAt
            };
        }

        public async Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId)
        {
            // UnitOfWork üzerinden verileri çekiyoruz
            var allWorkspaces = await _unitOfWork.Workspaces.ListAsync();

            // Geçici filtreleme (İleride Repo içine özel sorgu yazılabilir)
            // Kullanıcının sahip olduğu VEYA üye olduğu alanları getirmeliyiz
            // Şimdilik sadece Owner olduğu alanları getiriyoruz:
            var userWorkspaces = allWorkspaces
                .Where(w => w.OwnerId == userId)
                .Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    Description = w.Description,
                    OwnerId = w.OwnerId,
                    IsPrivate = w.IsPrivate,
                    CreatedAt = w.CreatedAt
                });

            return userWorkspaces;
        }

        public async Task AddMemberAsync(AddMemberDto dto)
        {
            // İleride burası da UnitOfWork ile yapılacak
            await Task.CompletedTask;
        }
    }
}