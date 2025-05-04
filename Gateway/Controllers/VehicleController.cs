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
        public async Task<IActionResult> Register([FromBody] RegisterVehicleRequest registerRequest)
        {
            var response = await _vehicleServiceClient.RegisterAsync(registerRequest);

            if (response.Status != "OK")
            {
                return BadRequest("Failed to register vehicle.");
            }

            return Ok(new { Message = "Vehicle registered successfully." });
        }

        // Endpoint para obtener un vehículo por placa
        [HttpGet("{plateNumber}")]
        public async Task<IActionResult> GetByPlate([Required] string plateNumber)
        {
            var request = new GetVehicleRequest { PlateNumber = plateNumber };
            var response = await _vehicleServiceClient.GetByPlateAsync(request);

            if (string.IsNullOrEmpty(response.PlateNumber))
            {
                return NotFound("Vehicle not found.");
            }

            return Ok(response);
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