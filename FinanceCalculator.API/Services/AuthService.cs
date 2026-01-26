using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Data;
using FinanceCalculator.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinanceCalculator.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        public AuthService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return null;

            var normalized = request.Username.Trim().ToLowerInvariant();
            var exists = await _db.Users.AnyAsync(u => u.Username == normalized);
            if (exists) return null;

            CreatePasswordHash(request.Password, out var hash, out var salt);
            var isFirstUser = !await _db.Users.AnyAsync();
            var user = new User
            {
                Username = normalized,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = isFirstUser ? "Admin" : "User"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await LogAuditAsync(user.Id, "Register");

            return new AuthResponse
            {
                Username = user.Username,
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var normalized = request.Username.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == normalized);
            if (user == null) return null;

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return null;

            await LogAuditAsync(user.Id, "Login");

            return new AuthResponse
            {
                Username = user.Username,
                Token = GenerateJwtToken(user)
            };
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(storedHash);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? "FinanceCalculator";
            var audience = jwtSection.GetValue<string>("Audience") ?? "FinanceCalculator";

            var jti = Guid.NewGuid().ToString("N");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task LogAuditAsync(int userId, string @event)
        {
            await _db.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Event = @event,
                TimestampUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
