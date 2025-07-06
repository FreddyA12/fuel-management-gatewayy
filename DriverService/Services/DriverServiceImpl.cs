using DriverService.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Services
{
    public class DriverServiceImpl : DriverService.DriverServiceBase
    {
        private readonly DriverDbContext _db;

        public DriverServiceImpl(DriverDbContext db)
        {
            _db = db;
        }

        public override async Task<RegisterDriverResponse> Register(RegisterDriverRequest request, ServerCallContext context)
        {
            try
            {
                Console.WriteLine($"Intentando registrar chofer: {request.IdentificationNumber}");

                var exists = await _db.Drivers.AnyAsync(d => d.IdentificationNumber == request.IdentificationNumber);
                if (exists)
                {
                    Console.WriteLine("Chofer ya existe.");
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "El chofer ya está registrado"));
                }

                var driver = new Driver
                {
                    Name = request.Name,
                    IdentificationNumber = request.IdentificationNumber,
                    Available = request.Available,
                    MachineryType = request.MachineryType
                };

                _db.Drivers.Add(driver);
                await _db.SaveChangesAsync();

                Console.WriteLine("Registro exitoso.");
                return new RegisterDriverResponse { Status = "OK" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR Register] {ex.Message}\n{ex.StackTrace}");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al registrar chofer."));
            }
        }


        public override async Task<GetDriverResponse> GetByIdentification(GetDriverRequest request, ServerCallContext context)
        {
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.IdentificationNumber == request.IdentificationNumber);

            if (driver is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Chofer no encontrado"));

            return new GetDriverResponse
            {
                Name = driver.Name,
                IdentificationNumber = driver.IdentificationNumber,
                Available = driver.Available,
                MachineryType = driver.MachineryType
            };
        }

        public override async Task<UpdateDriverResponse> Update(UpdateDriverRequest request, ServerCallContext context)
        {
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.IdentificationNumber == request.IdentificationNumber);

            if (driver is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Chofer no encontrado"));

            driver.Name = request.Name;
            driver.Available = request.Available;
            driver.MachineryType = request.MachineryType;

            await _db.SaveChangesAsync();

            return new UpdateDriverResponse { Status = "Updated" };
        }

        public override async Task<DeleteDriverResponse> Delete(DeleteDriverRequest request, ServerCallContext context)
        {
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.IdentificationNumber == request.IdentificationNumber);

            if (driver is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Chofer no encontrado"));

            _db.Drivers.Remove(driver);
            await _db.SaveChangesAsync();

            return new DeleteDriverResponse { Status = "Deleted" };
        }

        public override async Task<ListAllDriversResponse> ListAll(Empty request, ServerCallContext context)
        {
            var drivers = await _db.Drivers.ToListAsync();

            var response = new ListAllDriversResponse();
            response.Drivers.AddRange(drivers.Select(d => new DriverItem
            {
                Name = d.Name,
                IdentificationNumber = d.IdentificationNumber,
                Available = d.Available,
                MachineryType = d.MachineryType
            }));

            return response;
        }
    }
}
