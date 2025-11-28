using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkspaceService> _logger;

        public WorkspaceService(IUnitOfWork unitOfWork, ILogger<WorkspaceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId)
        {
            _logger.LogInformation("Creating workspace '{Title}' for user {UserId}", dto.Title, userId);

            var workspace = new Workspace
            {
                Title = dto.Title,
                Description = dto.Description,
                IsPrivate = dto.IsPrivate,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                Members = new List<WorkspaceMember>
                {
                    new WorkspaceMember { UserId = userId, Role = "Admin", JoinedAt = DateTime.UtcNow }
                }
            };

            await _unitOfWork.Workspaces.AddAsync(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Workspace created. Id: {WorkspaceId}", workspace.Id);

            return MapToDto(workspace);
        }

        // Yeni: Get By ID (Güvenli)
        public async Task<WorkspaceDto> GetByIdAsync(int id, Guid userId)
        {
            // İleride Repository'de Include(w => w.Members) içeren özel metot yazılmalı.
            // Şimdilik basit ID ile çekip kontrol ediyoruz.
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);

            if (workspace == null) throw new Exception("Workspace not found");

            // Erişim kontrolü: Sahibi mi veya Üyesi mi? (Members listesi dolu gelmeli!)
            // Basitlik için sadece Owner kontrolü veya Public kontrolü yapıyoruz şimdilik.
            if (workspace.IsPrivate && workspace.OwnerId != userId)
            {
                // Üyelik kontrolü Repository seviyesinde yapılmalı, burada basit bir koruma:
                throw new UnauthorizedAccessException("You do not have access to this workspace");
            }

            return MapToDto(workspace);
        }

        public async Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId)
        {
            var allWorkspaces = await _unitOfWork.Workspaces.ListAsync();

            // Performans notu: Repository'ye GetByOwnerAsync metodu eklenmeli.
            var userWorkspaces = allWorkspaces
                .Where(w => w.OwnerId == userId) // Üye olduklarını da eklemek lazım ileride
                .Select(MapToDto);

            return userWorkspaces;
        }

        // Yeni: Update
        public async Task UpdateAsync(int id, WorkspaceUpdateDto dto, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId != userId)
            {
                _logger.LogWarning("Unauthorized update attempt. User: {UserId}, Workspace: {WorkspaceId}", userId, id);
                throw new UnauthorizedAccessException("Only the owner can update the workspace");
            }

            if (dto.Title != null) workspace.Title = dto.Title;
            if (dto.Description != null) workspace.Description = dto.Description;

            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Workspace {WorkspaceId} updated by {UserId}", id, userId);
        }

        // Yeni: Delete
        public async Task DeleteAsync(int id, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) return;

            if (workspace.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the owner can delete the workspace");
            }

            _unitOfWork.Workspaces.Delete(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Workspace {WorkspaceId} deleted by {UserId}", id, userId);
        }

        public async Task AddMemberAsync(AddMemberDto dto, Guid currentUserId)
        {
            // Burada yetki kontrolü ve ekleme mantığı olacak
            // Şimdilik pass geçiyoruz veya implemente edebiliriz.
            _logger.LogInformation("Adding member {NewUserId} to workspace", dto.UserId);
            await Task.CompletedTask;
        }

        // Mapper Helper
        private static WorkspaceDto MapToDto(Workspace w)
        {
            return new WorkspaceDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                IsPrivate = w.IsPrivate,
                CreatedAt = w.CreatedAt,
                OwnerId = w.OwnerId
            };
        }
    }
}