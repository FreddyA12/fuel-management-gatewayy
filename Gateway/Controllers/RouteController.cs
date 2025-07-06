using Microsoft.AspNetCore.Mvc;
using RouteService.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly RouteService.Grpc.RouteService.RouteServiceClient _routeServiceClient;
        private readonly ILogger<RouteController> _logger;

        public RouteController(RouteService.Grpc.RouteService.RouteServiceClient routeServiceClient, ILogger<RouteController> logger)
        {
            _routeServiceClient = routeServiceClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRouteRequest request)
        {
            _logger.LogInformation("Intentando registrar ruta con nombre: {RouteName}", request.Name);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Fallo al registrar ruta: nombre vacío");
                return BadRequest("El nombre de la ruta es obligatorio.");
            }

            try
            {
                var result = await _routeServiceClient.RegisterAsync(request);
                if (result.Status == "OK")
                {
                    _logger.LogInformation("Ruta registrada exitosamente: {RouteName}", request.Name);
                    return Ok("Ruta registrada exitosamente.");
                }
                else
                {
                    _logger.LogWarning("Error al registrar ruta: {RouteName}", request.Name);
                    return BadRequest("Error al registrar la ruta.");
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al registrar ruta: {RouteName}", request.Name);
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Solicitando ruta por ID: {RouteId}", id);

            try
            {
                var request = new GetRouteRequest { Id = id };
                var response = await _routeServiceClient.GetByIdAsync(request);

                _logger.LogInformation("Ruta encontrada: {RouteId}", id);
                return Ok(response);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al obtener ruta por ID: {RouteId}", id);
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateRouteRequest updateRequest)
        {
            _logger.LogInformation("Intentando actualizar ruta ID: {RouteId}", updateRequest.Id);

            try
            {
                var response = await _routeServiceClient.UpdateAsync(updateRequest);

                if (response.Status == "Updated")
                {
                    _logger.LogInformation("Ruta actualizada exitosamente: {RouteId}", updateRequest.Id);
                    return Ok("Ruta actualizada exitosamente.");
                }
                else
                {
                    _logger.LogWarning("Error al actualizar ruta: {RouteId}", updateRequest.Id);
                    return BadRequest("Error al actualizar la ruta.");
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al actualizar ruta: {RouteId}", updateRequest.Id);
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Intentando eliminar ruta ID: {RouteId}", id);

            try
            {
                var response = await _routeServiceClient.DeleteAsync(new DeleteRouteRequest { Id = id });

                if (response.Status == "Deleted")
                {
                    _logger.LogInformation("Ruta eliminada exitosamente: {RouteId}", id);
                    return Ok("Ruta eliminada exitosamente.");
                }
                else
                {
                    _logger.LogWarning("Error al eliminar ruta: {RouteId}", id);
                    return BadRequest("Error al eliminar la ruta.");
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al eliminar ruta: {RouteId}", id);
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Solicitando listado completo de rutas");

            try
            {
                var response = await _routeServiceClient.ListAllAsync(new Empty());

                if (response.Routes.Count == 0)
                {
                    _logger.LogInformation("No hay rutas registradas");
                    return NotFound("No hay rutas registradas.");
                }

                _logger.LogInformation("Se encontraron {Count} rutas", response.Routes.Count);
                return Ok(response.Routes);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al listar rutas");
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }
    }
}
