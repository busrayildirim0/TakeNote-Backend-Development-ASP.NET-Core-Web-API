using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IWorkspaceService
    {
        // Temel CRUD
        Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId);
        Task<WorkspaceDto> GetByIdAsync(int id, Guid userId);

        // Listeleme: Hem benimkiler hem de Public olanlar
        Task<IEnumerable<WorkspaceDto>> GetAvailableWorkspacesAsync(Guid userId);

        Task UpdateAsync(int id, WorkspaceUpdateDto dto, Guid userId);
        Task DeleteAsync(int id, Guid userId);

        // Üye Yönetimi (Yeni Metotlar)
        Task AddMemberAsync(int workspaceId, AddMemberDto dto, Guid currentUserId); // Sadece Admin
        Task RemoveMemberAsync(int workspaceId, Guid memberIdToRemove, Guid currentUserId); // Sadece Admin

        // Katılım İşlemleri (Yeni Metotlar)
        Task JoinAsync(int workspaceId, Guid userId); // Herkes (Public ise)
        Task LeaveAsync(int workspaceId, Guid userId); // Herkes (Kendi isteğiyle)
    }
}