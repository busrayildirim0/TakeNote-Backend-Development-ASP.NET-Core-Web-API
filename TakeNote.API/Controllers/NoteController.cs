using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TakeNote.API.Hubs;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NoteController> _logger;
        private readonly IHubContext<CollaborationHub> _hubContext;

        public NoteController(INoteService noteService, ILogger<NoteController> logger, IHubContext<CollaborationHub> hubContext)
        {
            _noteService = noteService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(NoteCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _noteService.CreateAsync(dto, userId);

            // Yeni not eklendiğini çalışma alanındakilere duyurabiliriz (Burada basitleştirildi)
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _noteService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("workspace/{workspaceId}")]
        public async Task<IActionResult> GetAllByWorkspace(int workspaceId)
        {
            var result = await _noteService.GetAllByWorkspaceAsync(workspaceId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NoteUpdateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            await _noteService.UpdateAsync(id, dto, userId);

            // Bu notu izleyen herkese "Not Güncellendi" sinyali gönder
            await _hubContext.Clients.Group(id.ToString()).SendAsync("ReceiveNoteUpdate", id, dto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _noteService.DeleteAsync(id);
            await _hubContext.Clients.Group(id.ToString()).SendAsync("ReceiveNoteDelete", id);
            return NoContent();
        }
    }
}