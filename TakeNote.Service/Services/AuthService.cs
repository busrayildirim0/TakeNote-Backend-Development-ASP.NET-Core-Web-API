using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TakeNote.Core.Entities;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // IUnitOfWork'ü sildik çünkü Identity veritabanı işlerini kendi halleder.
        public AuthService(UserManager<User> userManager, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserAuthResponseDto> RegisterAsync(UserRegisterDto dto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

            // 1. Kullanıcı Nesnesini Oluştur
            // Not: Şifreyi buraya KOYMUYORUZ. CreateAsync metodu şifreyi alıp kendisi hashleyecek.
            var user = new User
            {
                UserName = dto.Username, // IdentityUser 'UserName' alanını kullanır
                Email = dto.Email
            };

            // 2. Identity Kullanarak Oluştur (Şifreleme ve Kaydetme Otomatik)
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                // Hata varsa (Örn: Şifre yetersiz, email kayıtlı) logla ve fırlat
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Register failed for {Email}: {Errors}", dto.Email, errors);
                throw new Exception($"Registration failed: {errors}");
            }

            // (Opsiyonel) Rol ekleme işlemi buraya gelebilir:
            // await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User registered successfully. UserId: {UserId}", user.Id);

            return GenerateAuthResponse(user);
        }

        public async Task<UserAuthResponseDto> LoginAsync(UserLoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            // 1. Kullanıcıyı Email ile Bul
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // 2. Kullanıcı var mı ve Şifre Doğru mu? (CheckPasswordAsync şifreyi doğrular)
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning("Login failed. Invalid credentials for email: {Email}", dto.Email);
                throw new Exception("Invalid email or password");
            }

            _logger.LogInformation("User logged in successfully. UserId: {UserId}", user.Id);
            return GenerateAuthResponse(user);
        }

        private UserAuthResponseDto GenerateAuthResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!), // Nullable uyarısını geçmek için !
                    new Claim(ClaimTypes.Name, user.UserName!)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new UserAuthResponseDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}