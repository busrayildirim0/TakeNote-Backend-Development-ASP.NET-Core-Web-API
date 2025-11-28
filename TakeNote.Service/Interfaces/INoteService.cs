using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface INoteService
    {
        Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId);
        Task<NoteDto> GetByIdAsync(int id);
        Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId);
        Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId);
        Task DeleteAsync(int id);
    }
}