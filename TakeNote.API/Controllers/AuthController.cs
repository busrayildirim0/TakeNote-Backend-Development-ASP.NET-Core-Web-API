using Microsoft.AspNetCore.Mvc;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; // Logger

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return CreatedAtAction(nameof(Login), new { email = result.Email }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for {Email}", dto.Email);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Login failed for {Email}", dto.Email); 
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}