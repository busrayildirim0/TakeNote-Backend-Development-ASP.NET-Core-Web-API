using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TakeNote.API.Hubs;

namespace TakeNote.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CollaborationController : ControllerBase
    {
        private readonly IHubContext<CollaborationHub> _hubContext;
        private readonly ILogger<CollaborationController> _logger;

        public CollaborationController(
            IHubContext<CollaborationHub> hubContext,
            ILogger<CollaborationController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("typing/{noteId}")]
        public async Task<IActionResult> SendTypingIndicator(string noteId)
        {
            _logger.LogInformation(
                "Typing indicator triggered for NoteId: {NoteId}", noteId);

            await _hubContext.Clients
                .Group($"note_{noteId}")
                .SendAsync("UserTyping", noteId);

            _logger.LogInformation(
                "Typing indicator sent via SignalR for NoteId: {NoteId}", noteId);

            return Ok();
        }
    }
}
