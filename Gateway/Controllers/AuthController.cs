using Authentication.Grpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService.AuthServiceClient _authServiceClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService.AuthServiceClient authServiceClient, ILogger<AuthController> logger)
        {
            _authServiceClient = authServiceClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            _logger.LogInformation("Intentando registrar usuario: {Username}", registerRequest.Username);

            try
            {
                var response = await _authServiceClient.RegisterAsync(registerRequest);

                if (response.Status != "OK")
                {
                    _logger.LogWarning(" Falló el registro de usuario: {Username}", registerRequest.Username);
                    return BadRequest("Failed to register user.");
                }

                _logger.LogInformation(" Usuario registrado exitosamente: {Username}", registerRequest.Username);
                return Ok(new { Message = "User registered successfully." });
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, " Error gRPC al registrar usuario {Username}: {Detail}", registerRequest.Username, ex.Status.Detail);
                return StatusCode(500, "Error interno de autenticación.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation("➡ Intentando login para: {Username}", loginRequest.Username);

            try
            {
                var response = await _authServiceClient.LoginAsync(loginRequest);

                if (response.Status != 200)
                {
                    _logger.LogWarning("Login fallido para {Username}: {Error}", loginRequest.Username, response.Error);
                    return BadRequest(response.Error);
                }

                _logger.LogInformation(" Login exitoso para {Username}", loginRequest.Username);
                return Ok(new { Token = response.Token });
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, " Error gRPC al hacer login de {Username}: {Detail}", loginRequest.Username, ex.Status.Detail);
                return StatusCode(500, "Error interno de autenticación.");
            }
        }
    }
}
