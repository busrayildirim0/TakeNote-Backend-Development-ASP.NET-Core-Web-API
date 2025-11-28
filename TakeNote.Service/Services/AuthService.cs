using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Utilities;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<UserAuthResponseDto> RegisterAsync(UserRegisterDto dto)
        {
            var existingUsers = await _unitOfWork.Users.ListAsync(u => u.Email == dto.Email);
            if (existingUsers.Any())
                throw new Exception("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                Roles = new List<UserRole> { new UserRole { RoleName = "User" } }
            };

            await _unitOfWork.Users.AddAsync(user);
            // UnitOfWork olmadığı için burada SaveChanges gerekebilir ama GenericRepo içinde AddAsync SaveChanges yapıyorsa gerek yok. 
            // Bizim EfRepository'de SaveChanges yoktu, UnitOfWork kullanmalıyız.
            // **Hızlı çözüm için:** Generic Repo'ya SaveChanges eklemiş varsayıyorum veya Controller'da UnitOfWork çağıracağız.
            // *Doğrusu:* Repository'den sonra _unitOfWork.CompleteAsync() çağırmaktır. Şimdilik mock yapıda ilerliyoruz.

            await _unitOfWork.CompleteAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<UserAuthResponseDto> LoginAsync(UserLoginDto dto)
        {
            var users = await _unitOfWork.Users.ListAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();

            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            return GenerateAuthResponse(user);
        }

        private UserAuthResponseDto GenerateAuthResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new UserAuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}