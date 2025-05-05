using VehicleService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FuelManagementGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService.VehicleService.VehicleServiceClient _vehicleServiceClient;

        public VehicleController(VehicleService.VehicleService.VehicleServiceClient vehicleServiceClient)
        {
            _vehicleServiceClient = vehicleServiceClient;
        }

        // Endpoint para registrar un nuevo vehículo
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVehicleRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlateNumber))
                return BadRequest("Datos incompletos");

            try
            {
                // Intentar buscar el vehículo
                try
                {
                    var exists = await _vehicleServiceClient.GetByPlateAsync(
                        new GetVehicleRequest { PlateNumber = request.PlateNumber.Trim().ToUpper() });

                    if (!string.IsNullOrEmpty(exists?.PlateNumber))
                        return Conflict("La placa ya existe");
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    // El vehículo no existe, está bien, seguimos con el registro
                }

                // Registrar el nuevo vehículo
                var result = await _vehicleServiceClient.RegisterAsync(request);

                return result?.Status == "OK"
                    ? Ok("Registro exitoso")
                    : BadRequest("Error en el registro");
            }
            catch (Grpc.Core.RpcException rpcEx)
            {
                return StatusCode(500, $"Error gRPC: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }


        // Endpoint para obtener un vehículo por placa
        [HttpGet("{plateNumber}")]
        public async Task<IActionResult> GetByPlate([Required] string plateNumber)
        {
            try
            {
                var request = new GetVehicleRequest { PlateNumber = plateNumber };
                var response = await _vehicleServiceClient.GetByPlateAsync(request);

                if (string.IsNullOrEmpty(response.PlateNumber))
                {
                    return NotFound(new { message = "Vehículo no encontrado" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error interno del servidor PLACA " });
            }
        }

        // Endpoint para actualizar un vehículo
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateVehicleRequest updateRequest)
        {
            var response = await _vehicleServiceClient.UpdateAsync(updateRequest);

            if (response.Status != "Updated")
            {
                return BadRequest("Failed to update vehicle.");
            }

            return Ok(new { Message = "Vehicle updated successfully." });
        }

        // Endpoint para eliminar un vehículo
        [HttpDelete("{plateNumber}")]
        public async Task<IActionResult> Delete([Required] string plateNumber)
        {
            var request = new DeleteVehicleRequest { PlateNumber = plateNumber };
            var response = await _vehicleServiceClient.DeleteAsync(request);

            if (response.Status != "Deleted")
            {
                return BadRequest("Failed to delete vehicle.");
            }

            return Ok(new { Message = "Vehicle deleted successfully." });
        }

        // Endpoint para listar todos los vehículos
        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            var request = new Empty();
            var response = await _vehicleServiceClient.ListAllAsync(request);

            if (response.Vehicles.Count == 0)
            {
                return NotFound("No vehicles found.");
            }

            return Ok(response.Vehicles);
        }
    }
}