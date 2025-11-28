using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId);
        Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(Guid userId);
        Task AddMemberAsync(AddMemberDto dto);
    }
}