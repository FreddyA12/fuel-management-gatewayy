using Authentication.Grpc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService.AuthServiceClient _authServiceClient;

        public AuthController(AuthService.AuthServiceClient authServiceClient)
        {
            _authServiceClient = authServiceClient;
        }

        // Endpoint para registrar un nuevo usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var response = await _authServiceClient.RegisterAsync(registerRequest);

            if (response.Status != "Success")
            {
                return BadRequest("Failed to register user.");
            }

            return Ok(new { Message = "User registered successfully." });
        }

        // Endpoint para hacer login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _authServiceClient.LoginAsync(loginRequest);

            if (response.Status != 200)
            {
                return BadRequest(response.Error);
            }

            return Ok(new { Token = response.Token });
        }
    }
}
