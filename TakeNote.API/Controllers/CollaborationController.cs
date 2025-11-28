//(Aktif kullanıcılar veya manuel sinyaller için)
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

        // Test amaçlı: Bir nota "Yazıyor..." bildirimi göndermek için endpoint
        [HttpPost("typing/{noteId}")]
        public async Task<IActionResult> SendTypingIndicator(string noteId)
        {
            // İstemciden (Frontend) SignalR ile direkt yapılabilir ama API üzerinden de tetiklenebilir
            await _hubContext.Clients.Group(noteId).SendAsync("UserTyping", noteId);
            return Ok();
        }
    }
}