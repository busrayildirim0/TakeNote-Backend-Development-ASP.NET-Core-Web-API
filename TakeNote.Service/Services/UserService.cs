// TakeNote.Service/Services/UserService.cs - GÜNCELLENMİŞ
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            UserManager<User> userManager,
            ILogger<UserService> logger,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found");

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found");

            bool updated = false;

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.UserName)
            {
                user.UserName = dto.Username;
                updated = true;
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
                updated = true;
            }

            if (updated)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Update failed: {errors}");
                }
            }

            _logger.LogInformation("User profile updated: {UserId}", userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!
            };
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            // 1. Kişisel notları sil (Hard Delete)
            var personalNotes = await _unitOfWork.Notes.ListAsync(n =>
                n.CreatedById == userId &&
                n.WorkspaceId == null);

            foreach (var note in personalNotes)
            {
                _unitOfWork.Notes.Delete(note);
            }

            // 2. Ortak alanlardaki notlar için CreatedBy'ı null yap (Ghost User)
            var sharedNotes = await _unitOfWork.Notes.ListAsync(n =>
                n.CreatedById == userId &&
                n.WorkspaceId != null);

            foreach (var note in sharedNotes)
            {
                note.CreatedById = Guid.Empty; // Veya özel bir "Deleted User" ID'si
                _unitOfWork.Notes.Update(note);
            }

            await _unitOfWork.CompleteAsync();

            // 3. Kullanıcıyı sil
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Account deletion failed: {errors}");
            }

            _logger.LogInformation("User account deleted: {UserId}", userId);
        }
    }
}