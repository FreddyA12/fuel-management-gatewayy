using Microsoft.AspNetCore.Mvc;
using FuelService.Grpc;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelConsumptionController : ControllerBase
    {
        private readonly FuelConsumptionService.FuelConsumptionServiceClient _fuelServiceClient;

        public FuelConsumptionController(FuelConsumptionService.FuelConsumptionServiceClient fuelServiceClient)
        {
            _fuelServiceClient = fuelServiceClient;
        }

        // ?? Registrar un nuevo consumo de combustible
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterFuelConsumptionRequest request)
        {
            if (request.VehicleId <= 0 || request.RouteId <= 0 || request.DriverId <= 0 || string.IsNullOrWhiteSpace(request.Date))
                return BadRequest("Datos incompletos o inválidos.");

            try
            {
                var result = await _fuelServiceClient.RegisterAsync(request);
                return result.Status == "OK"
                    ? Ok("Consumo registrado exitosamente.")
                    : BadRequest("Error al registrar el consumo.");
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        // ?? Listar todos los registros de consumo
        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            try
            {
                var response = await _fuelServiceClient.ListAllAsync(new Empty());
                if (response.Consumptions.Count == 0)
                    return NotFound("No hay registros de consumo.");

                return Ok(response.Consumptions);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }
    }
}
