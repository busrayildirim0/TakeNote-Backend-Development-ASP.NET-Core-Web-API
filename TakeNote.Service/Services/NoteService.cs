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

        public NoteService(IUnitOfWork unitOfWork, ILogger<NoteService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // CREATE: Not Oluşturma
        public async Task<NoteDto> CreateAsync(NoteCreateDto dto, Guid userId)
        {
            // SENARYO 1: Ortak Not (WorkspaceId DOLU ise)
            if (dto.WorkspaceId.HasValue)
            {
                var workspace = await _unitOfWork.Workspaces.GetByIdAsync(dto.WorkspaceId.Value);
                if (workspace == null) throw new Exception("Workspace not found");

                // [GERÇEK KONTROL]: Kullanıcı bu workspace'in sahibi mi veya üyesi mi?
                // Not: Generic Repository 'GetByIdAsync' metodu 'Members' listesini Include etmeyebilir.
                // Bu yüzden garanti olsun diye veritabanından üyeliği sorguluyoruz.

                // Eğer workspace sahibi ise sorun yok. Değilse üye listesine bak.
                if (workspace.OwnerId != userId)
                {
                    // WorkspaceMember tablosuna erişmek için Workspace üzerinden gidiyoruz.
                    // (EF Core ile 'Members' yüklü gelmediyse diye güvenli kontrol)
                    var userWorkspaces = await _unitOfWork.Workspaces.ListAsync(w =>
                        w.Id == dto.WorkspaceId.Value &&
                        w.Members.Any(m => m.UserId == userId));

                    if (!userWorkspaces.Any())
                    {
                        _logger.LogWarning("User {UserId} tried to create note in Workspace {WsId} without membership.", userId, dto.WorkspaceId);
                        throw new UnauthorizedAccessException("You must be a member of the workspace to create a note there.");
                    }
                }
            }

            // SENARYO 2: Kişisel Not (WorkspaceId BOŞ ise) -> Kontrolsüz devam.

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

            return MapToDto(note);
        }

        // GET BY ID: Tekil Not Getirme
        public async Task<NoteDto> GetByIdAsync(int id, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdWithRelationsAsync(id);
            if (note == null) throw new Exception("Note not found");

            // ERİŞİM KONTROLÜ
            bool hasAccess = false;

            if (note.WorkspaceId == null)
            {
                // 1. Kişisel Not: Sadece sahibi görebilir
                if (note.CreatedById == userId) hasAccess = true;
            }
            else
            {
                // 2. Ortak Not: Sahibi veya Üyeler görebilir
                var workspace = await _unitOfWork.Workspaces.GetByIdAsync(note.WorkspaceId.Value);

                if (workspace != null)
                {
                    if (workspace.OwnerId == userId)
                    {
                        hasAccess = true;
                    }
                    else
                    {
                        // Üyelik kontrolü (Veritabanından teyitli)
                        var isMember = (await _unitOfWork.Workspaces.ListAsync(w =>
                            w.Id == workspace.Id &&
                            w.Members.Any(m => m.UserId == userId))).Any();

                        if (isMember) hasAccess = true;
                    }
                }
            }

            if (!hasAccess)
            {
                _logger.LogWarning("Unauthorized access attempt to Note {NoteId} by User {UserId}", id, userId);
                throw new UnauthorizedAccessException("You do not have permission to view this note.");
            }

            return MapToDto(note);
        }

        // GET PERSONAL: Kişisel Notlar
        public async Task<IEnumerable<NoteDto>> GetPersonalNotesAsync(Guid userId)
        {
            var notes = await _unitOfWork.Notes.ListAsync(n => n.CreatedById == userId && n.WorkspaceId == null);
            return notes.Select(MapToDto);
        }

        // GET WORKSPACE: Ortak Notlar
        public async Task<IEnumerable<NoteDto>> GetAllByWorkspaceAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            // ERİŞİM KONTROLÜ: Private workspace ise ve üye değilse hata ver
            if (workspace.IsPrivate && workspace.OwnerId != userId)
            {
                var isMember = (await _unitOfWork.Workspaces.ListAsync(w =>
                    w.Id == workspaceId &&
                    w.Members.Any(m => m.UserId == userId))).Any();

                if (!isMember)
                {
                    throw new UnauthorizedAccessException("You cannot access notes of a private workspace you are not a member of.");
                }
            }

            var notes = await _unitOfWork.Notes.GetNotesByWorkspaceAsync(workspaceId);
            return notes.Select(MapToDto);
        }

        // UPDATE: Güncelleme
        public async Task UpdateAsync(int id, NoteUpdateDto dto, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null) throw new Exception("Note not found");

            // YETKİ KONTROLÜ
            if (note.WorkspaceId == null)
            {
                // KİŞİSEL NOT: Sadece sahibi güncelleyebilir
                if (note.CreatedById != userId)
                    throw new UnauthorizedAccessException("You cannot edit someone else's personal note.");
            }
            else
            {
                // ORTAK NOT: Sahibi VEYA Workspace Üyeleri güncelleyebilir
                // Önce Workspace'i bulalım
                var workspace = await _unitOfWork.Workspaces.GetByIdAsync(note.WorkspaceId.Value);
                if (workspace == null) throw new Exception("Workspace not found");

                bool canEdit = false;
                if (workspace.OwnerId == userId) canEdit = true; // Admin her şeyi düzenler
                else if (note.CreatedById == userId) canEdit = true; // Oluşturan düzenler
                else
                {
                    // Üye mi? (Üye ise düzenleyebilir - İşbirliği Modu)
                    var isMember = (await _unitOfWork.Workspaces.ListAsync(w =>
                        w.Id == workspace.Id &&
                        w.Members.Any(m => m.UserId == userId))).Any();

                    if (isMember) canEdit = true;
                }

                if (!canEdit) throw new UnauthorizedAccessException("You do not have permission to edit this note.");
            }

            if (dto.Title != null) note.Title = dto.Title;
            if (dto.Content != null) note.Content = dto.Content;
            if (dto.IsPinned.HasValue) note.IsPinned = dto.IsPinned.Value;
            if (dto.IsLocked.HasValue) note.IsLocked = dto.IsLocked.Value;

            note.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Notes.Update(note);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Note {NoteId} updated by {UserId}.", id, userId);
        }

        // DELETE: Silme
        public async Task DeleteAsync(int id, Guid userId)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null) return;

            if (note.WorkspaceId == null)
            {
                // KİŞİSEL NOT: Sadece sahibi silebilir
                if (note.CreatedById != userId)
                    throw new UnauthorizedAccessException("You cannot delete someone else's personal note.");
            }
            else
            {
                // ORTAK NOT: Sadece Sahibi VEYA Workspace Admini silebilir
                var workspace = await _unitOfWork.Workspaces.GetByIdAsync(note.WorkspaceId.Value);
                if (workspace == null) return; // Workspace yoksa not da yetim kalmıştır, silinebilir belki ama güvenli duralım.

                bool canDelete = false;
                if (note.CreatedById == userId) canDelete = true; // Kendi notumu silerim
                if (workspace.OwnerId == userId) canDelete = true; // Adminsem herkesinkini silerim

                if (!canDelete)
                {
                    throw new UnauthorizedAccessException("Only the creator or workspace admin can delete this note.");
                }
            }

            _unitOfWork.Notes.Delete(note);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Note {NoteId} deleted by {UserId}.", id, userId);
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