// TakeNote.Service/Interfaces/IUserService.cs - GÜNCELLENMİŞ
using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UserUpdateDto dto);
        Task DeleteAccountAsync(Guid userId);
    }
}