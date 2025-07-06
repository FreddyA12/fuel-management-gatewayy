using DriverService;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly DriverService.DriverService.DriverServiceClient _driverClient;

        public DriverController(DriverService.DriverService.DriverServiceClient driverClient)
        {
            _driverClient = driverClient;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDriverRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdentificationNumber))
                return BadRequest("Datos incompletos");

            try
            {
                // Comprobar si ya existe
                try
                {
                    var exists = await _driverClient.GetByIdentificationAsync(
                        new GetDriverRequest { IdentificationNumber = request.IdentificationNumber });

                    if (!string.IsNullOrEmpty(exists?.IdentificationNumber))
                        return Conflict("El chofer ya está registrado");
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    // No existe, continuar
                }

                var result = await _driverClient.RegisterAsync(request);
                return result?.Status == "OK" ? Ok("Registro exitoso") : BadRequest("Error en el registro");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpGet("{identificationNumber}")]
        public async Task<IActionResult> GetByIdentification([Required] string identificationNumber)
        {
            try
            {
                var request = new GetDriverRequest { IdentificationNumber = identificationNumber };
                var response = await _driverClient.GetByIdentificationAsync(request);

                if (string.IsNullOrEmpty(response.IdentificationNumber))
                    return NotFound(new { message = "Chofer no encontrado" });

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateDriverRequest request)
        {
            var result = await _driverClient.UpdateAsync(request);
            return result.Status == "Updated"
                ? Ok(new { message = "Chofer actualizado correctamente" })
                : BadRequest("No se pudo actualizar");
        }

        [HttpDelete("{identificationNumber}")]
        public async Task<IActionResult> Delete([Required] string identificationNumber)
        {
            var result = await _driverClient.DeleteAsync(new DeleteDriverRequest { IdentificationNumber = identificationNumber });
            return result.Status == "Deleted"
                ? Ok(new { message = "Chofer eliminado correctamente" })
                : BadRequest("No se pudo eliminar");
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            var response = await _driverClient.ListAllAsync(new Empty());
            return response.Drivers.Count == 0
                ? NotFound("No hay choferes registrados")
                : Ok(response.Drivers);
        }
    }
}
