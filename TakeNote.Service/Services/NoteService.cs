using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;
// Hub'ın olduğu namespace'i eklemeyi unutmayın (TakeNote.API referansı Service'de olamaz,
// Bu yüzden Hub arayüzünü soyutlamak gerekir ama basitlik için IHubContext<Hub> yerine
// string metodlarla kullanacağız veya Hub'ı Core/Service katmanından ayıracağız.
// PRATİK ÇÖZÜM: IHubContext'i burada kullanmak için SignalR Core paketini Service'e eklemek gerekir.
// Terminal: dotnet add TakeNote.Service/TakeNote.Service.csproj package Microsoft.AspNetCore.SignalR.Core

namespace TakeNote.Service.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NoteService> _logger;
        // Gerçek projede Hub Client arayüzü (IClientProxy) kullanmak daha temizdir ama
        // burada generic HubContext kullanıyoruz.
        // Not: Eğer Service katmanı API'yi görmüyorsa (ki görmemeli), Hub sınıfı burada tanınmaz.
        // Bu yüzden SignalR bildirimini Controller'da yapmak mimari açıdan daha kolaydır.
        // Ancak iş mantığı burada olduğu için biz "return" edeceğiz, Controller sinyal çakacak.

        public NoteService(IUnitOfWork unitOfWork, ILogger<NoteService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId)
        {
            _logger.LogInformation("Creating new note for user {UserId} in workspace {WorkspaceId}", userId, dto.WorkspaceId);

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content ?? "",
                WorkspaceId = dto.WorkspaceId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                IsPinned = dto.IsPinned
            };

            await _unitOfWork.Notes.AddAsync(note);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Note {NoteId} created successfully.", note.Id);

            return MapToDto(note);
        }

        public async Task<NoteDto> GetByIdAsync(int id)
        {
            var note = await _unitOfWork.Notes.GetByIdWithRelationsAsync(id);
            if (note == null) throw new Exception("Note not found");

            return MapToDto(note);
        }

        public async Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId)
        {
            // NoteRepository'deki özel metodu kullanıyoruz
            var notes = await _unitOfWork.Notes.GetNotesByWorkspaceAsync(workspaceId);
            return notes.Select(MapToDto);
        }

        public async Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null) throw new Exception("Note not found");

            // Basit yetki kontrolü (Daha iyisi Policy ile olur)
            // if (note.CreatedById != userId) throw new UnauthorizedAccessException("Not owner");

            if (dto.Title != null) note.Title = dto.Title;
            if (dto.Content != null) note.Content = dto.Content;
            if (dto.IsPinned.HasValue) note.IsPinned = dto.IsPinned.Value;
            if (dto.IsLocked.HasValue) note.IsLocked = dto.IsLocked.Value;

            note.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Notes.Update(note);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Note {NoteId} updated.", id);
        }

        public async Task DeleteAsync(int id)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null) return; // Zaten yok

            _unitOfWork.Notes.Delete(note);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Note {NoteId} deleted.", id);
        }

        private static NoteDto MapToDto(Note note)
        {
            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                IsPinned = note.IsPinned,
                WorkspaceId = note.WorkspaceId,
                CreatedById = note.CreatedById,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}