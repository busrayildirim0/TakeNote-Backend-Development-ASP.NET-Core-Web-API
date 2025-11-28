using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class UserService : IUserService
    {
        // UnitOfWork yerine UserManager kullanıyoruz
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<User> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UserAuthResponseDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                throw new Exception("User not found");
            }

            return new UserAuthResponseDto
            {
                UserId = user.Id,
                Username = user.UserName!, // IdentityUser'da 'UserName' kullanılır
                Email = user.Email!
            };
        }

        public async Task UpdateUserAsync(Guid id, UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new Exception("User not found");

            // Değişiklikleri uygula
            if (!string.IsNullOrEmpty(dto.Username)) user.UserName = dto.Username;
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;

            // Identity ile güncelleme işlemi (UnitOfWork yerine)
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User update failed: {Errors}", errors);
                throw new Exception($"User update failed: {errors}");
            }

            _logger.LogInformation("User updated profile. UserId: {UserId}", id);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return;

            // Identity ile silme işlemi
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User deletion failed: {Errors}", errors);
                throw new Exception($"User deletion failed: {errors}");
            }

            _logger.LogInformation("User deleted account. UserId: {UserId}", id);
        }
    }
}