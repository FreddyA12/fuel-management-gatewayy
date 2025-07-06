using DriverService;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly DriverService.DriverService.DriverServiceClient _driverClient;
        private readonly ILogger<DriverController> _logger;

        public DriverController(DriverService.DriverService.DriverServiceClient driverClient, ILogger<DriverController> logger)
        {
            _driverClient = driverClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDriverRequest request)
        {
            _logger.LogInformation("Intentando registrar chofer con cédula: {Cedula}", request.IdentificationNumber);

            if (request == null || string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                _logger.LogWarning(" Registro fallido: Datos incompletos");
                return BadRequest("Datos incompletos");
            }

            try
            {
                try
                {
                    var exists = await _driverClient.GetByIdentificationNumberAsync(
                        new IdentificationNumberRequest { IdentificationNumber = request.IdentificationNumber });

                    if (!string.IsNullOrEmpty(exists?.IdentificationNumber))
                    {
                        _logger.LogWarning(" Chofer ya registrado con cédula: {Cedula}", request.IdentificationNumber);
                        return Conflict(new { message = "El chofer ya está registrado con esa cédula." });
                    }
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    _logger.LogInformation(" Chofer no encontrado, se procederá a registrarlo.");
                }

                var result = await _driverClient.RegisterAsync(request);

                if (result?.Status == "OK")
                {
                    _logger.LogInformation(" Chofer registrado exitosamente: {Cedula}", request.IdentificationNumber);
                    return Ok("Registro exitoso");
                }

                _logger.LogWarning(" Error al registrar chofer: {Cedula}", request.IdentificationNumber);
                return BadRequest("Error en el registro");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Excepción al registrar chofer");
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([Required] int id)
        {
            _logger.LogInformation(" Buscando chofer con ID: {Id}", id);

            try
            {
                var request = new GetDriverRequest { Id = id };
                var response = await _driverClient.GetByIdAsync(request);

                if (response == null || response.Id == 0)
                {
                    _logger.LogWarning("Chofer no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Chofer no encontrado" });
                }

                _logger.LogInformation("Chofer encontrado: {Id}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener chofer por ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateDriverRequest request)
        {
            _logger.LogInformation("Intentando actualizar chofer con ID: {Id}", request.Id);

            var result = await _driverClient.UpdateAsync(request);
            if (result.Status == "Updated")
            {
                _logger.LogInformation("✅ Chofer actualizado correctamente: {Id}", request.Id);
                return Ok(new { message = "Chofer actualizado correctamente" });
            }

            _logger.LogWarning("Falló la actualización del chofer: {Id}", request.Id);
            return BadRequest("No se pudo actualizar");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([Required] int id)
        {
            _logger.LogInformation("Intentando eliminar chofer con ID: {Id}", id);

            var result = await _driverClient.DeleteAsync(new DeleteDriverRequest { Id = id });
            if (result.Status == "Deleted")
            {
                _logger.LogInformation("Chofer eliminado correctamente: {Id}", id);
                return Ok(new { message = "Chofer eliminado correctamente" });
            }

            _logger.LogWarning("Falló la eliminación del chofer: {Id}", id);
            return BadRequest("No se pudo eliminar");
        }

        [HttpGet("all")]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation(" Listando todos los choferes");

            var response = await _driverClient.ListAllAsync(new Empty());

            if (response.Drivers.Count == 0)
            {
                _logger.LogWarning(" No hay choferes registrados");
                return NotFound("No hay choferes registrados");
            }

            _logger.LogInformation(" {Count} choferes encontrados", response.Drivers.Count);
            return Ok(response.Drivers);
        }
    }
}
