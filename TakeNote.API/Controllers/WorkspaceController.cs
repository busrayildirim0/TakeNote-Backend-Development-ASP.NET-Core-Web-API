using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakeNote.Service.Interfaces;
using TakeNote.Service.DTOs;

namespace TakeNote.API.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar
    [ApiController]
    [Route("api/[controller]")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkspaceCreateDto dto)
        {
            // Token'dan UserId'yi alıyoruz
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var result = await _workspaceService.CreateAsync(dto, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _workspaceService.GetUserWorkspacesAsync(userId);
            return Ok(result);
        }
    }
}