using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface ITaskItemService
    {
        Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto);
        Task ToggleCompleteAsync(int id);
        Task DeleteAsync(int id);
    }
}