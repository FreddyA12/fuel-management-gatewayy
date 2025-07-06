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
                try
                {
                    var exists = await _driverClient.GetByIdentificationNumberAsync(
                        new IdentificationNumberRequest { IdentificationNumber = request.IdentificationNumber });

                    if (!string.IsNullOrEmpty(exists?.IdentificationNumber))
                        return Conflict(new { message = "El chofer ya está registrado con esa cédula." });
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    // No existe, se puede continuar con el registro
                }

                var result = await _driverClient.RegisterAsync(request);

                if (result?.Status == "OK")
                    return Ok("Registro exitoso");

                return BadRequest("Error en el registro");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([Required] int id)
        {
            try
            {
                var request = new GetDriverRequest { Id = id };
                var response = await _driverClient.GetByIdAsync(request); 

                if (response == null || response.Id == 0)
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([Required] int id)
        {
            var result = await _driverClient.DeleteAsync(new DeleteDriverRequest { Id = id });
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
