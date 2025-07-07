using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VehicleService.Models;

namespace VehicleService.Services
{
    public class VehicleServiceImpl : VehicleService.VehicleServiceBase
    {
        private readonly VehicleDbContext _db;

        public VehicleServiceImpl(VehicleDbContext db)
        {
            _db = db;
        }

        public override async Task<RegisterVehicleResponse> Register(RegisterVehicleRequest request, ServerCallContext context)
        {
            var vehicle = new Vehicle
            {
                PlateNumber = request.PlateNumber,
                MachineryType = request.MachineryType,
                IsOperational = request.IsOperational,
                FuelCapacity = request.FuelCapacity,
                RegisteredAt = DateTime.Parse(request.RegisteredAt),
                FuelConsumptionByKm = request.FuelConsumptionByKm,
                Model = request.Model
            };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();

            return new RegisterVehicleResponse { Status = "OK" };
        }

        public override async Task<GetVehicleResponse> GetByPlate(GetVehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == request.PlateNumber);

            if (vehicle is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Vehículo no encontrado"));

            return new GetVehicleResponse
            {
                Id = vehicle.Id, // ← Nuevo campo
                PlateNumber = vehicle.PlateNumber,
                MachineryType = vehicle.MachineryType,
                IsOperational = vehicle.IsOperational,
                FuelCapacity = vehicle.FuelCapacity,
                RegisteredAt = vehicle.RegisteredAt.ToString("yyyy-MM-dd"),
                FuelConsumptionByKm = vehicle.FuelConsumptionByKm,
                Model = vehicle.Model
            };

        }

        public override async Task<UpdateVehicleResponse> Update(UpdateVehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == request.PlateNumber);

            if (vehicle is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Vehículo no encontrado"));

            vehicle.MachineryType = request.MachineryType;
            vehicle.IsOperational = request.IsOperational;
            vehicle.FuelCapacity = request.FuelCapacity;
            vehicle.RegisteredAt = DateTime.Parse(request.RegisteredAt);
            vehicle.FuelConsumptionByKm = request.FuelConsumptionByKm;
            vehicle.Model = request.Model;

            await _db.SaveChangesAsync();

            return new UpdateVehicleResponse { Status = "Updated" };
        }

        public override async Task<DeleteVehicleResponse> Delete(DeleteVehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == request.PlateNumber);

            if (vehicle is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Vehículo no encontrado"));

            _db.Vehicles.Remove(vehicle);
            await _db.SaveChangesAsync();

            return new DeleteVehicleResponse { Status = "Deleted" };
        }

        public override async Task<ListAllVehiclesResponse> ListAll(Empty request, ServerCallContext context)
        {
            var vehicles = await _db.Vehicles.ToListAsync();

            var response = new ListAllVehiclesResponse();
            response.Vehicles.AddRange(vehicles.Select(v => new VehicleItem
            {
                Id = v.Id, // ← Nuevo campo
                PlateNumber = v.PlateNumber,
                MachineryType = v.MachineryType,
                IsOperational = v.IsOperational,
                FuelCapacity = v.FuelCapacity,
                RegisteredAt = v.RegisteredAt.ToString("yyyy-MM-dd"),
                FuelConsumptionByKm = v.FuelConsumptionByKm,
                Model = v.Model
            }));



            return response;
        }
    }
}
