using TakeNote.Service.DTOs; // DTO'ları görmesi için referans gerekebilir

namespace TakeNote.Service.Interfaces
{
    public interface IAuthService
    {
        Task<UserAuthResponseDto> RegisterAsync(UserRegisterDto dto);
        Task<UserAuthResponseDto> LoginAsync(UserLoginDto dto);
    }
}