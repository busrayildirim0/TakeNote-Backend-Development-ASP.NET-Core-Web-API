using TakeNote.Service.DTOs;

namespace TakeNote.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserAuthResponseDto> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(Guid id, UserUpdateDto dto); // Yeni
        Task DeleteUserAsync(Guid id); // Yeni
    }
}