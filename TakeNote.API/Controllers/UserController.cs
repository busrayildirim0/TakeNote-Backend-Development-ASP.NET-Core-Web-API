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
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return userId;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            _logger.LogInformation("GetProfile called by UserId: {UserId}", userId);

            var result = await _userService.GetProfileAsync(userId);

            _logger.LogInformation("Profile retrieved successfully for UserId: {UserId}", userId);
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UserUpdateDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("UpdateProfile called by UserId: {UserId}", userId);

            var result = await _userService.UpdateProfileAsync(userId, dto);

            _logger.LogInformation("Profile updated successfully for UserId: {UserId}", userId);
            return Ok(result);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserId();
            _logger.LogWarning("DeleteAccount called by UserId: {UserId}", userId);

            await _userService.DeleteAccountAsync(userId);

            _logger.LogWarning("Account deleted for UserId: {UserId}", userId);
            return NoContent();
        }
    }
}
