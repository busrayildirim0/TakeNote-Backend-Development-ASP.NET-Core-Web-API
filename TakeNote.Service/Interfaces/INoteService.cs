using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface INoteService
    {
        Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId);
        Task<NoteDto> GetByIdAsync(int id, Guid userId); // userId eklendi (Yetki kontrolü için)
        Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId, Guid userId); // userId eklendi
        Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId);
        Task DeleteAsync(int id, Guid userId); // userId eklendi
        Task<IEnumerable<NoteDto>> GetPersonalNotesAsync(Guid userId);
    }
}