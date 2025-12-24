// TakeNote.API/Controllers/UserController.cs - YENİ DOSYA
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _userService.GetProfileAsync(GetUserId());
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UserUpdateDto dto)
        {
            var result = await _userService.UpdateProfileAsync(GetUserId(), dto);
            return Ok(result);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            await _userService.DeleteAccountAsync(GetUserId());
            return NoContent();
        }
    }
}