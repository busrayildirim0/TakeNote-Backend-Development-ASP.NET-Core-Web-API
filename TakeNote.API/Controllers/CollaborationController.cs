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

        public CollaborationController(IHubContext<CollaborationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("typing/{noteId}")]
        public async Task<IActionResult> SendTypingIndicator(string noteId)
        {
            await _hubContext.Clients.Group(noteId).SendAsync("UserTyping", noteId);
            return Ok();
        }
    }
}