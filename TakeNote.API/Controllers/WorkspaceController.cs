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

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // GET: Hem benimkiler hem Public olanlar
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

        // --- YENİ ENDPOINTLER ---

        // Admin Üye Ekler
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            try
            {
                await _workspaceService.AddMemberAsync(id, dto, GetUserId());
                return Ok(new { message = "Member added successfully" });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Admin Üye Çıkarır
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

        // Herkes Public Olana Katılır
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                await _workspaceService.JoinAsync(id, GetUserId());
                return Ok(new { message = "Joined workspace successfully" });
            }
            catch (UnauthorizedAccessException) { return Forbid(); } // Private ise 403
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Herkes Çıkabilir
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
    }
}