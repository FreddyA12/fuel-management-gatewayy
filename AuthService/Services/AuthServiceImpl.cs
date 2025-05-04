using authentication.Models;
using Authentication.Grpc;
using BCrypt.Net;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace authentication.Services
{
    public class AuthServiceImpl : AuthService.AuthServiceBase
    {
        private readonly AuthDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthServiceImpl(AuthDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new RegisterResponse { Status = "Conflict: Usuario ya existe" };
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Enum.TryParse<Role>(request.Role, out var role) ? role : Role.Operador
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return new RegisterResponse { Status = "OK" };
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new LoginResponse
                {
                    Error = "Credenciales incorrectas",
                    Status = 401
                };
            }

            var token = GenerateToken(user);
            return new LoginResponse { Token = token, Status = 200 };
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim("username", user.Username),
                new Claim("role", user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
