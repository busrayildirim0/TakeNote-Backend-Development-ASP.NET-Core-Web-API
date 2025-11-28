using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserAuthResponseDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            return new UserAuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }
    }
}