// TakeNote.Service/Interfaces/INoteService.cs - BASITLEŞTIRILMIŞ
using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface INoteService
    {
        Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId);
        Task<NoteDto> GetByIdAsync(int id, Guid userId);
        Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId, Guid userId);
        Task<IEnumerable<NoteDto>> GetPersonalNotesAsync(Guid userId);
        Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId);
        Task DeleteAsync(int id, Guid userId); // Direkt kalıcı silme

        // Arama ve Filtreleme
        Task<IEnumerable<NoteDto>> SearchNotesAsync(Guid userId, int? workspaceId, string? query, bool? pinnedOnly, DateTime? createdAfter, bool? assignedToMe);
    }
}