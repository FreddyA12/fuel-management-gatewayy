using Microsoft.AspNetCore.Mvc;
using RouteService.Grpc;
using Grpc.Core;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly RouteService.Grpc.RouteService.RouteServiceClient _routeServiceClient;

        public RouteController(RouteService.Grpc.RouteService.RouteServiceClient routeServiceClient)
        {
            _routeServiceClient = routeServiceClient;
        }

        // 📌 Registrar una nueva ruta
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRouteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("El nombre de la ruta es obligatorio.");

            try
            {
                var result = await _routeServiceClient.RegisterAsync(request);
                return result.Status == "OK"
                    ? Ok("Ruta registrada exitosamente.")
                    : BadRequest("Error al registrar la ruta.");
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        // 📌 Obtener ruta por ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var request = new GetRouteRequest { Id = id };
                var response = await _routeServiceClient.GetByIdAsync(request);

                return Ok(response);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        // 📌 Actualizar una ruta
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateRouteRequest updateRequest)
        {
            try
            {
                var response = await _routeServiceClient.UpdateAsync(updateRequest);
                return response.Status == "Updated"
                    ? Ok("Ruta actualizada exitosamente.")
                    : BadRequest("Error al actualizar la ruta.");
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        // 📌 Eliminar una ruta por ID
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _routeServiceClient.DeleteAsync(new DeleteRouteRequest { Id = id });
                return response.Status == "Deleted"
                    ? Ok("Ruta eliminada exitosamente.")
                    : BadRequest("Error al eliminar la ruta.");
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        // 📌 Listar todas las rutas
        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            try
            {
                var response = await _routeServiceClient.ListAllAsync(new Empty());
                if (response.Routes.Count == 0)
                    return NotFound("No hay rutas registradas.");

                return Ok(response.Routes);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }
    }
}
