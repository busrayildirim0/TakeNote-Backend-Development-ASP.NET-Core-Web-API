using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemController : ControllerBase
    {
        private readonly ITaskItemService _taskService;
        private readonly ILogger<TaskItemController> _logger;

        public TaskItemController(
            ITaskItemService taskService,
            ILogger<TaskItemController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItemCreateDto dto)
        {
            var userId = GetUserId();

            _logger.LogInformation(
                "Create Task requested by UserId: {UserId} for NoteId: {NoteId}",
                userId,
                dto.NoteId);

            var result = await _taskService.CreateAsync(dto, userId);

            _logger.LogInformation(
                "Task created successfully. TaskId: {TaskId}, UserId: {UserId}",
                result.Id,
                userId);

            return Ok(result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            _logger.LogInformation(
                "ToggleComplete requested for TaskId: {TaskId}",
                id);

            await _taskService.ToggleCompleteAsync(id);

            _logger.LogInformation(
                "Task completion toggled successfully for TaskId: {TaskId}",
                id);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskItemUpdateDto dto)
        {
            _logger.LogInformation(
                "Update Task requested for TaskId: {TaskId}",
                id);

            var result = await _taskService.UpdateAsync(id, dto);

            _logger.LogInformation(
                "Task updated successfully for TaskId: {TaskId}",
                id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning(
                "Delete Task requested for TaskId: {TaskId}",
                id);

            await _taskService.DeleteAsync(id);

            _logger.LogWarning(
                "Task deleted successfully for TaskId: {TaskId}",
                id);

            return NoContent();
        }
    }
}
