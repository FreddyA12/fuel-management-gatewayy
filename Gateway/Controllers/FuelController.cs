using Microsoft.AspNetCore.Mvc;
using FuelService.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelConsumptionController : ControllerBase
    {
        private readonly FuelConsumptionService.FuelConsumptionServiceClient _fuelServiceClient;
        private readonly ILogger<FuelConsumptionController> _logger;

        public FuelConsumptionController(
            FuelConsumptionService.FuelConsumptionServiceClient fuelServiceClient,
            ILogger<FuelConsumptionController> logger)
        {
            _fuelServiceClient = fuelServiceClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterFuelConsumptionRequest request)
        {
            _logger.LogInformation("Iniciando registro de consumo. VehículoId: {VehicleId}, ChoferId: {DriverId}, RutaId: {RouteId}", request.VehicleId, request.DriverId, request.RouteId);

            if (request.VehicleId <= 0 || request.RouteId <= 0 || request.DriverId <= 0 || string.IsNullOrWhiteSpace(request.Date))
            {
                _logger.LogWarning("Registro fallido: Datos incompletos o inválidos.");
                return BadRequest("Datos incompletos o inválidos.");
            }

            try
            {
                var result = await _fuelServiceClient.RegisterAsync(request);

                if (result.Status == "OK")
                {
                    _logger.LogInformation("Consumo registrado correctamente.");
                    return Ok("Consumo registrado exitosamente.");
                }

                _logger.LogWarning("El servicio gRPC devolvió un estado diferente a OK.");
                return BadRequest("Error al registrar el consumo.");
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al registrar consumo.");
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Solicitando listado de todos los consumos.");

            try
            {
                var response = await _fuelServiceClient.ListAllAsync(new Empty());

                if (response.Consumptions.Count == 0)
                {
                    _logger.LogInformation("No se encontraron registros de consumo.");
                    return NotFound("No hay registros de consumo.");
                }

                _logger.LogInformation("Listado de consumos obtenido correctamente. Total: {Count}", response.Consumptions.Count);
                return Ok(response.Consumptions);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al obtener lista de consumos.");
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }

        [HttpPost("finalize/{id}")]
        public async Task<IActionResult> FinalizeTrip([FromRoute][Required] int id)
        {
            _logger.LogInformation("Intentando finalizar viaje con ID: {TripId}", id);

            if (id <= 0)
            {
                _logger.LogWarning("ID de viaje inválido: {TripId}", id);
                return BadRequest("ID inválido.");
            }

            try
            {
                var response = await _fuelServiceClient.FinalizeTripAsync(new FinalizeTripRequest { Id = id });

                if (response.Status == "OK")
                {
                    _logger.LogInformation("Viaje finalizado correctamente. ID: {TripId}", id);
                    return Ok("Viaje finalizado exitosamente.");
                }

                _logger.LogWarning("No se pudo finalizar el viaje con ID: {TripId}", id);
                return BadRequest("No se pudo finalizar el viaje.");
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                _logger.LogWarning("Viaje no encontrado para ID: {TripId}", id);
                return NotFound("Viaje no encontrado.");
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al finalizar viaje.");
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
            }
        }
    }
}
