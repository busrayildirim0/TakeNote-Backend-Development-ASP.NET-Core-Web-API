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

        public TaskItemService(IUnitOfWork unitOfWork, ILogger<TaskItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // CREATE: Görev eklerken Notun erişimini kontrol et
        public async Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto, Guid userId) // UserId ekledik
        {
            // 1. Notu bul
            var parentNote = await _unitOfWork.Notes.GetByIdAsync(dto.NoteId);
            if (parentNote == null) throw new Exception("Note not found");

            // 2. Not Kişisel mi? Erişim kontrolü
            if (parentNote.WorkspaceId == null)
            {
                if (parentNote.CreatedById != userId)
                    throw new UnauthorizedAccessException("Cannot add task to someone else's personal note.");
            }
            // Workspace notu ise, workspace üyeliği kontrol edilebilir.

            var task = new TaskItem
            {
                NoteId = dto.NoteId,
                Description = dto.Description,
                DueDate = dto.DueDate,
                AssignedToId = dto.AssignedToId,
                IsCompleted = false
            };

            await _unitOfWork.TaskItems.AddAsync(task);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Task {TaskId} added to Note {NoteId}", task.Id, dto.NoteId);

            return new TaskItemDto
            {
                Id = task.Id,
                NoteId = task.NoteId,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                DueDate = task.DueDate,
                AssignedToId = task.AssignedToId
            };
        }

        public async Task ToggleCompleteAsync(int id)
        {
            var task = await _unitOfWork.TaskItems.GetByIdAsync(id);
            if (task == null) throw new Exception("Task not found");

            task.IsCompleted = !task.IsCompleted;
            _unitOfWork.TaskItems.Update(task);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _unitOfWork.TaskItems.GetByIdAsync(id);
            if (task == null) return;

            _unitOfWork.TaskItems.Delete(task);
            await _unitOfWork.CompleteAsync();
        }
    }
}