using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId);
        Task<WorkspaceDto> GetByIdAsync(int id, Guid userId); // Yeni (Güvenlik için userId şart)
        Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId);
        Task UpdateAsync(int id, WorkspaceUpdateDto dto, Guid userId); // Yeni
        Task DeleteAsync(int id, Guid userId); // Yeni
        Task AddMemberAsync(AddMemberDto dto, Guid currentUserId); // Güncellendi (Yetki kontrolü için)
    }
}