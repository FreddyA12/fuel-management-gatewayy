using VehicleService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService.VehicleService.VehicleServiceClient _vehicleServiceClient;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(VehicleService.VehicleService.VehicleServiceClient vehicleServiceClient, ILogger<VehicleController> logger)
        {
            _vehicleServiceClient = vehicleServiceClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVehicleRequest request)
        {
            _logger.LogInformation("Intentando registrar vehículo con placa: {PlateNumber}", request?.PlateNumber);

            if (request == null || string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                _logger.LogWarning("Datos incompletos para registro de vehículo.");
                return BadRequest("Datos incompletos");
            }

            try
            {
                try
                {
                    var exists = await _vehicleServiceClient.GetByPlateAsync(
                        new GetVehicleRequest { PlateNumber = request.PlateNumber.Trim().ToUpper() });

                    if (!string.IsNullOrEmpty(exists?.PlateNumber))
                    {
                        _logger.LogWarning("Intento de registrar vehículo con placa existente: {PlateNumber}", request.PlateNumber);
                        return Conflict("La placa ya existe");
                    }
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    _logger.LogInformation("No existe vehículo con placa: {PlateNumber}, procediendo al registro", request.PlateNumber);
                }

                var result = await _vehicleServiceClient.RegisterAsync(request);

                if (result?.Status == "OK")
                {
                    _logger.LogInformation("Vehículo registrado exitosamente: {PlateNumber}", request.PlateNumber);
                    return Ok("Registro exitoso");
                }
                else
                {
                    _logger.LogWarning("Error al registrar vehículo: {PlateNumber}", request.PlateNumber);
                    return BadRequest("Error en el registro");
                }
            }
            catch (Grpc.Core.RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "Error gRPC en registro vehículo: {PlateNumber}", request.PlateNumber);
                return StatusCode(500, $"Error gRPC: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en registro vehículo");
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpGet("{plateNumber}")]
        public async Task<IActionResult> GetByPlate([Required] string plateNumber)
        {
            _logger.LogInformation("Solicitando vehículo con placa: {PlateNumber}", plateNumber);

            try
            {
                var request = new GetVehicleRequest { PlateNumber = plateNumber };
                var response = await _vehicleServiceClient.GetByPlateAsync(request);

                if (string.IsNullOrEmpty(response.PlateNumber))
                {
                    _logger.LogWarning("Vehículo no encontrado: {PlateNumber}", plateNumber);
                    return NotFound(new { message = "Vehículo no encontrado" });
                }

                _logger.LogInformation("Vehículo encontrado: {PlateNumber}", plateNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno del servidor al buscar vehículo con placa: {PlateNumber}", plateNumber);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateVehicleRequest updateRequest)
        {
           

            try
            {
                var response = await _vehicleServiceClient.UpdateAsync(updateRequest);

                if (response.Status != "Updated")
                {
                    return BadRequest("Failed to update vehicle.");
                }

                return Ok(new { Message = "Vehicle updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpDelete("{plateNumber}")]
        public async Task<IActionResult> Delete([Required] string plateNumber)
        {
            _logger.LogInformation("Intentando eliminar vehículo con placa: {PlateNumber}", plateNumber);

            try
            {
                var request = new DeleteVehicleRequest { PlateNumber = plateNumber };
                var response = await _vehicleServiceClient.DeleteAsync(request);

                if (response.Status != "Deleted")
                {
                    _logger.LogWarning("No se pudo eliminar vehículo con placa: {PlateNumber}", plateNumber);
                    return BadRequest("Failed to delete vehicle.");
                }

                _logger.LogInformation("Vehículo eliminado exitosamente con placa: {PlateNumber}", plateNumber);
                return Ok(new { Message = "Vehicle deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al eliminar vehículo con placa: {PlateNumber}", plateNumber);
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Solicitando listado de todos los vehículos");

            try
            {
                var request = new Empty();
                var response = await _vehicleServiceClient.ListAllAsync(request);

                if (response.Vehicles.Count == 0)
                {
                    _logger.LogInformation("No hay vehículos registrados");
                    return NotFound("No vehicles found.");
                }

                _logger.LogInformation("Se encontraron {Count} vehículos", response.Vehicles.Count);
                return Ok(response.Vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al listar vehículos");
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }
    }
}
