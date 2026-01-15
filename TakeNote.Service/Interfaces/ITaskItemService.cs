using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface ITaskItemService
    {
        // userId parametresini buraya da ekle
        Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto, Guid userId);
        Task ToggleCompleteAsync(int id);
        Task DeleteAsync(int id);
        Task<TaskItemDto> UpdateAsync(int id, TaskItemUpdateDto dto);
    }
}