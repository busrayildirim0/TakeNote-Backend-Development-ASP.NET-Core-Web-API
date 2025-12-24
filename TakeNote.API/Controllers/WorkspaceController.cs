using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly ILogger<WorkspaceController> _logger;

        public WorkspaceController(IWorkspaceService workspaceService, ILogger<WorkspaceController> logger)
        {
            _workspaceService = workspaceService;
            _logger = logger;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _workspaceService.GetAvailableWorkspacesAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _workspaceService.GetByIdAsync(id, GetUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkspaceCreateDto dto)
        {
            var result = await _workspaceService.CreateAsync(dto, GetUserId());
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkspaceUpdateDto dto)
        {
            try
            {
                await _workspaceService.UpdateAsync(id, dto, GetUserId());
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _workspaceService.DeleteAsync(id, GetUserId());
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ADD MEMBER - DEBUG LOGGING EKLENDI
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            try
            {
                _logger.LogInformation("AddMember Request: WorkspaceId={Id}, DTO={@Dto}", id, dto);

                if (dto == null)
                {
                    _logger.LogError("AddMemberDto is NULL!");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrEmpty(dto.UserIdentifier) && dto.UserId == Guid.Empty)
                {
                    _logger.LogError("Both UserIdentifier and UserId are empty!");
                    return BadRequest(new { message = "UserIdentifier veya UserId gereklidir" });
                }

                await _workspaceService.AddMemberAsync(id, dto, GetUserId());
                return Ok(new { message = "Member added successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddMember Error");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, Guid userId)
        {
            try
            {
                await _workspaceService.RemoveMemberAsync(id, userId, GetUserId());
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                await _workspaceService.JoinAsync(id, GetUserId());
                return Ok(new { message = "Joined workspace successfully" });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            try
            {
                await _workspaceService.LeaveAsync(id, GetUserId());
                return Ok(new { message = "Left workspace successfully" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicWorkspaces()
        {
            var result = await _workspaceService.GetPublicWorkspacesAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            try
            {
                var result = await _workspaceService.GetMembersAsync(id, GetUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("{id}/my-role")]
        public async Task<IActionResult> GetMyRole(int id)
        {
            try
            {
                var role = await _workspaceService.GetUserRoleAsync(id, GetUserId());
                return Ok(new { role });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}