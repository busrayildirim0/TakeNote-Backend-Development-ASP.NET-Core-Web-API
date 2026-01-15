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

        // Helper: Token'dan User ID okuma (Kod tekrarını önlemek için)
        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        [HttpPost]
        public async Task<IActionResult> Create(NoteCreateDto dto)
        {
            var userId = GetUserId();
            var result = await _noteService.CreateAsync(dto, userId);

            // [FIX]: Sadece Ortak Not ise (WorkspaceId varsa) bildirim gönder.
            // Kişisel notlar için gruba bildirim atılmaz.
            if (dto.WorkspaceId.HasValue)
            {
                await _hubContext.Clients.Group($"workspace_{dto.WorkspaceId}")
                    .SendAsync("ReceiveNewNote", result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Service'e userId gönderiyoruz ki yetki kontrolü yapabilsin
            var result = await _noteService.GetByIdAsync(id, GetUserId());
            return Ok(result);
        }

        [HttpGet("personal")]
        public async Task<IActionResult> GetPersonalNotes()
        {
            var result = await _noteService.GetPersonalNotesAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("workspace/{workspaceId}")]
        public async Task<IActionResult> GetAllByWorkspace(int workspaceId)
        {
            // Duplicate metodu sildik, tek ve düzgün olan bu.
            // Ayrıca userId gönderiyoruz.
            var result = await _noteService.GetAllByWorkspaceAsync(workspaceId, GetUserId());
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NoteUpdateDto dto)
        {
            await _noteService.UpdateAsync(id, dto, GetUserId());

            // SADECE note metadata güncellendi bilgisi gider
            await _hubContext.Clients.Group($"note_{id}")
                .SendAsync("ReceiveNoteUpdate", id, dto);

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _noteService.DeleteAsync(id, GetUserId());

            // Silindi bilgisini gönder
            await _hubContext.Clients.Group($"note_{id}")
                .SendAsync("ReceiveNoteDelete", id);

            return NoContent();
        }


        [HttpGet("personal/search")]
        public async Task<IActionResult> SearchPersonalNotes(
            [FromQuery] string? query,
            [FromQuery] bool? pinnedOnly,
            [FromQuery] DateTime? createdAfter)
        {
            var result = await _noteService.SearchNotesAsync(
                GetUserId(), null, query, pinnedOnly, createdAfter, null);

            return Ok(result);
        }



        [HttpGet("workspace/{workspaceId}/search")]
        public async Task<IActionResult> SearchWorkspaceNotes(
            int workspaceId,
            [FromQuery] string? query,
            [FromQuery] bool? pinnedOnly,
            [FromQuery] DateTime? createdAfter,
            [FromQuery] bool? assignedToMe)
        {
            var result = await _noteService.SearchNotesAsync(
                GetUserId(), workspaceId, query, pinnedOnly, createdAfter, assignedToMe);
            return Ok(result);
        }
    }
}