using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface INotificationService
    {
        // Not İşlemleri
        Task NotifyNoteCreatedAsync(int workspaceId, NoteDto note);
        Task NotifyNoteUpdatedAsync(int noteId, int noteIdPayload, NoteUpdateDto dto); // Payload ve ID aynı olabilir
        Task NotifyNoteDeletedAsync(int noteId);

        // Görev İşlemleri (Task)
        Task NotifyTaskAddedAsync(int noteId, TaskItemDto task);
        Task NotifyTaskUpdatedAsync(int noteId, TaskItemDto task);
        Task NotifyTaskDeletedAsync(int noteId, int taskId);

        // Workspace İşlemleri
        Task NotifyMemberAddedAsync(int workspaceId, Guid userId);
    }
}