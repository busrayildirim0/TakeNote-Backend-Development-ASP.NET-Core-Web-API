using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;
using TakeNote.Core.Interfaces;

namespace TakeNote.Service.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NoteService> _logger;
        private readonly INotificationService _notificationService;

        public NoteService(
           IUnitOfWork unitOfWork,
           ILogger<NoteService> logger,
           INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        // CREATE
        public async Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId)
        {
            if (dto.WorkspaceId.HasValue)
            {
                var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(dto.WorkspaceId.Value);
                if (workspace == null) throw new Exception("Workspace not found");

                // ROL KONTROLÜ: Admin veya Editor NOT EKLEYEBİLİR
                var userRole = await GetUserRoleInWorkspace(workspace, userId);
                if (userRole != "Admin" && userRole != "Editor")
                    throw new UnauthorizedAccessException("Sadece Admin ve Editor not ekleyebilir.");
            }

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content ?? "",
                WorkspaceId = dto.WorkspaceId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPinned = dto.IsPinned,
                Tags = dto.Tags ?? new List<string>()
            };

            await _unitOfWork.Notes.AddAsync(note);
            await _unitOfWork.CompleteAsync();

            var createdNoteDto = await MapToDtoWithUsername(note);

            if (dto.WorkspaceId.HasValue)
            {
                await _notificationService.NotifyNoteCreatedAsync(dto.WorkspaceId.Value, createdNoteDto);
            }

            _logger.LogInformation("Note created. ID: {NoteId}", note.Id);
            return createdNoteDto;
        }

        // GET BY ID
        public async Task<NoteDto> GetByIdAsync(int id, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdWithRelationsAsync(id);
            if (note == null) throw new Exception("Note not found");

            bool hasAccess = await CheckNoteAccess(note, userId);
            if (!hasAccess) throw new UnauthorizedAccessException("Permission denied.");

            return await MapToDtoWithUsername(note);
        }

        // GET PERSONAL NOTES
        public async Task<IEnumerable<NoteDto>> GetPersonalNotesAsync(Guid userId)
        {
            var notes = await _unitOfWork.Notes.ListAsync(n =>
                n.CreatedById == userId && n.WorkspaceId == null);

            return await MapToDtoListWithUsername(notes);
        }

        // GET WORKSPACE NOTES
        public async Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            // ERIŞIM KONTROLÜ
            if (workspace.IsPrivate && workspace.OwnerId != userId)
            {
                var isMember = workspace.Members.Any(m => m.UserId == userId);
                if (!isMember) throw new UnauthorizedAccessException("Access denied.");
            }

            var notes = await _unitOfWork.Notes.ListAsync(n => n.WorkspaceId == workspaceId);
            return await MapToDtoListWithUsername(notes);
        }

        // UPDATE
        public async Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdWithRelationsAsync(id);
            if (note == null)
                throw new Exception("Note not found");

            bool canEdit = await CheckNoteEditPermission(note, userId);
            if (!canEdit)
                throw new UnauthorizedAccessException("Bu notu düzenleyemezsiniz.");

            if (dto.Title != null)
                note.Title = dto.Title;

            if (dto.Content != null)
                note.Content = dto.Content;

            if (dto.IsPinned.HasValue)
                note.IsPinned = dto.IsPinned.Value;

            if (dto.IsLocked.HasValue)
                note.IsLocked = dto.IsLocked.Value;

            if (dto.Tags != null)
                note.Tags = dto.Tags;

            note.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Notes.Update(note);
            await _unitOfWork.CompleteAsync();

            await _notificationService.NotifyNoteUpdatedAsync(id, id, dto);
            _logger.LogInformation("Note updated: {NoteId}", id);
        }
        // DELETE - DOĞRUDAN SİLİNİYOR (ÇÖP KUTUSU YOK)
        public async Task DeleteAsync(int id, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null) return;

            // YETKİ KONTROLÜ
            bool canDelete = await CheckNoteDeletePermission(note, userId);
            if (!canDelete) throw new UnauthorizedAccessException("Bu notu silemezsiniz.");

            _unitOfWork.Notes.Delete(note);
            await _unitOfWork.CompleteAsync();

            await _notificationService.NotifyNoteDeletedAsync(id);
            _logger.LogInformation("Note {NoteId} permanently deleted by {UserId}.", id, userId);
        }

        // SEARCH - GELİŞTİRİLMİŞ FİLTRELEME
        public async Task<IEnumerable<NoteDto>> SearchNotesAsync(
            Guid userId,
            int? workspaceId,
            string? query,
            bool? pinnedOnly,
            DateTime? createdAfter,
            bool? assignedToMe)
        {
            IEnumerable<Note> notes;

            if (workspaceId.HasValue)
            {
                notes = await _unitOfWork.Notes.ListAsync(n => n.WorkspaceId == workspaceId);
            }
            else
            {
                notes = await _unitOfWork.Notes.ListAsync(n =>
                    n.CreatedById == userId && n.WorkspaceId == null);
            }

            // ARAMA
            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                notes = notes.Where(n =>
                    n.Title.ToLower().Contains(query) ||
                    n.Content.ToLower().Contains(query) ||
                    n.Tags.Any(t => t.ToLower().Contains(query)));
            }

            // PİNLİ NOTLAR
            if (pinnedOnly == true)
            {
                notes = notes.Where(n => n.IsPinned);
            }

            // TARİH FİLTRESİ
            if (createdAfter.HasValue)
            {
                notes = notes.Where(n => n.CreatedAt >= createdAfter.Value);
            }

            
            if (assignedToMe == true && workspaceId.HasValue)
            {
                notes = notes.Where(n => n.Tasks.Any(t => t.AssignedToId == userId));
            }


            return await MapToDtoListWithUsername(notes);
        }

        // HELPER: ERIŞIM KONTROLÜ
        private async Task<bool> CheckNoteAccess(Note note, Guid userId)
        {
            if (note.WorkspaceId == null)
            {
                return note.CreatedById == userId;
            }

            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(note.WorkspaceId.Value);
            if (workspace == null) return false;

            if (workspace.OwnerId == userId) return true;

            var isMember = workspace.Members.Any(m => m.UserId == userId);
            return isMember;
        }

        // HELPER: DÜZENLEME YETKİSİ (Admin, Editor veya Yaratıcı)
        private async Task<bool> CheckNoteEditPermission(Note note, Guid userId)
        {
            if (note.WorkspaceId == null)
            {
                return note.CreatedById == userId;
            }

            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(note.WorkspaceId.Value);
            if (workspace == null) return false;

            var userRole = await GetUserRoleInWorkspace(workspace, userId);

            // Admin ve Editor düzenleyebilir, ayrıca not sahibi de düzenleyebilir
            return userRole == "Admin" || userRole == "Editor" || note.CreatedById == userId;
        }

        // HELPER: SİLME YETKİSİ (Admin veya Yaratıcı)
        private async Task<bool> CheckNoteDeletePermission(Note note, Guid userId)
        {
            if (note.WorkspaceId == null)
            {
                return note.CreatedById == userId;
            }

            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(note.WorkspaceId.Value);
            if (workspace == null) return false;

            var userRole = await GetUserRoleInWorkspace(workspace, userId);

            // Admin tüm notları silebilir, diğerleri sadece kendi notlarını
            return userRole == "Admin" || note.CreatedById == userId;
        }

        // HELPER: KULLANICI ROLÜNÜ AL
        private async Task<string> GetUserRoleInWorkspace(Workspace workspace, Guid userId)
        {
            if (workspace.OwnerId == userId) return "Admin";

            var member = workspace.Members.FirstOrDefault(m => m.UserId == userId);
            return member?.Role ?? "None";
        }

        // HELPER: DTO MAPPING
        private async Task<NoteDto> MapToDtoWithUsername(Note note)
        {
            var creator = await _unitOfWork.Notes.ListAsync(n => n.Id == note.Id);
            var creatorUser = creator.FirstOrDefault()?.CreatedBy;

            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                IsPinned = note.IsPinned,
                Tags = note.Tags ?? new List<string>(),
                WorkspaceId = note.WorkspaceId,
                CreatedById = note.CreatedById,
                CreatedByUsername = creatorUser?.UserName ?? "Unknown",
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                Tasks = note.Tasks?.Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    NoteId = t.NoteId,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    DueDate = t.DueDate,
                    AssignedToId = t.AssignedToId
                }).ToList() ?? new List<TaskItemDto>()
            };
        }

        private async Task<List<NoteDto>> MapToDtoListWithUsername(IEnumerable<Note> notes)
        {
            var result = new List<NoteDto>();
            foreach (var note in notes)
            {
                result.Add(await MapToDtoWithUsername(note));
            }
            return result;
        }
    }
}