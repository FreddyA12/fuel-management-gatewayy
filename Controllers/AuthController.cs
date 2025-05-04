using authentication.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace authentication.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthController(AuthDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return Conflict("Usuario ya existe");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new
                {
                    error = "Credenciales incorrectas",
                    status = 401
                });

            var token = GenerateToken(user);
            return Ok(new { token });
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

    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Role Role { get; set; } = Role.Operador;
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
