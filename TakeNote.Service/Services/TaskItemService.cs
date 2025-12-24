using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskItemService> _logger;
        private readonly INotificationService _notificationService;

        public TaskItemService(IUnitOfWork unitOfWork, ILogger<TaskItemService> logger, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto, Guid userId)
        {
            var parentNote = await _unitOfWork.Notes.GetByIdAsync(dto.NoteId);
            if (parentNote == null) throw new Exception("Note not found");

            if (parentNote.WorkspaceId == null && parentNote.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("Cannot add task to someone else's personal note.");
            }

            var task = new TaskItem
            {
                NoteId = dto.NoteId,
                Title = dto.Description, // ✅ TITLE EKLENDI
                Description = dto.Description,
                DueDate = dto.DueDate,
                AssignedToId = dto.AssignedToId,
                IsCompleted = false
            };

            await _unitOfWork.TaskItems.AddAsync(task);
            await _unitOfWork.CompleteAsync();

            var taskDto = MapToDto(task);
            await _notificationService.NotifyTaskAddedAsync(dto.NoteId, taskDto);

            _logger.LogInformation("Task {TaskId} added to Note {NoteId}", task.Id, dto.NoteId);
            return taskDto;
        }

        public async Task ToggleCompleteAsync(int id)
        {
            var task = await _unitOfWork.TaskItems.GetByIdAsync(id);
            if (task == null) throw new Exception("Task not found");

            task.IsCompleted = !task.IsCompleted;
            _unitOfWork.TaskItems.Update(task);
            await _unitOfWork.CompleteAsync();

            var taskDto = MapToDto(task);
            await _notificationService.NotifyTaskUpdatedAsync(task.NoteId, taskDto);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _unitOfWork.TaskItems.GetByIdAsync(id);
            if (task == null) return;

            int noteId = task.NoteId;
            _unitOfWork.TaskItems.Delete(task);
            await _unitOfWork.CompleteAsync();

            await _notificationService.NotifyTaskDeletedAsync(noteId, id);
        }

        private static TaskItemDto MapToDto(TaskItem task)
        {
            return new TaskItemDto
            {
                Id = task.Id,
                NoteId = task.NoteId,
                Title = task.Title, 
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                DueDate = task.DueDate,
                AssignedToId = task.AssignedToId
            };
        }
    }
}