// TakeNote.Service/Interfaces/IWorkspaceService.cs - GÜNCELLENMİŞ
using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId);
        Task<WorkspaceDto> GetByIdAsync(int id, Guid userId);
        Task<IEnumerable<WorkspaceDto>> GetAvailableWorkspacesAsync(Guid userId);
        Task<IEnumerable<WorkspaceDto>> GetPublicWorkspacesAsync(Guid userId); // userId parametresi eklendi
        Task UpdateAsync(int id, WorkspaceUpdateDto dto, Guid userId);
        Task DeleteAsync(int id, Guid userId);

        Task AddMemberAsync(int workspaceId, AddMemberDto dto, Guid currentUserId);
        Task RemoveMemberAsync(int workspaceId, Guid memberIdToRemove, Guid currentUserId);
        Task JoinAsync(int workspaceId, Guid userId);
        Task LeaveAsync(int workspaceId, Guid userId);

        // YENİ: Üye listesi
        Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId, Guid userId);
        Task<string> GetUserRoleAsync(int workspaceId, Guid userId); // YENİ
    }
}