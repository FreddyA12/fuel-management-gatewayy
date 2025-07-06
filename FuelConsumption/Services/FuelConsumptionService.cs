
using FuelConsumption.Models;
using FuelService.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using static FuelService.Grpc.FuelConsumptionService;


namespace FuelConsumption.Services
{
    public class FuelConsumptionServiceImpl : FuelConsumptionServiceBase
    {
        private readonly FuelConsumptionDbContext _db;

        public FuelConsumptionServiceImpl(FuelConsumptionDbContext db)
        {
            _db = db;
        }

        public override async Task<RegisterFuelConsumptionResponse> Register(RegisterFuelConsumptionRequest request, ServerCallContext context)
        {
            // Validar disponibilidad de vehículo
            var activeVehicleTrip = await _db.FuelConsumptions
                .FirstOrDefaultAsync(fc => fc.VehicleId == request.VehicleId && fc.State == "INICIADO");

            if (activeVehicleTrip != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "El vehículo ya está en un viaje activo."));
            }

            // Validar disponibilidad de chofer
            var activeDriverTrip = await _db.FuelConsumptions
                .FirstOrDefaultAsync(fc => fc.DriverId == request.DriverId && fc.State == "INICIADO");

            if (activeDriverTrip != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "El chofer ya está en un viaje activo."));
            }

            // Si están disponibles, registrar nuevo viaje
            var record = new Models.FuelConsumption
            {
                VehicleId = request.VehicleId,
                RouteId = request.RouteId,
                DriverId = request.DriverId,
                Date = DateTime.Parse(request.Date),
                ActualConsumption = request.ActualConsumption,
                EstimatedConsumptionPerKm = request.EstimatedConsumptionPerKm,
                State = "INICIADO",

               
            };

            //Cambiar el estado del chofer, avality a false

            var driver = await _db.Drivers.FindAsync(request.DriverId);
            if (driver != null)
            {
                driver.Available = false;
            }

            _db.FuelConsumptions.Add(record);
            await _db.SaveChangesAsync();

            return new RegisterFuelConsumptionResponse { Status = "OK" };
        }
        public override async Task<ListAllFuelConsumptionsResponse> ListAll(Empty request, ServerCallContext context)
        {
            var records = await _db.FuelConsumptions
                .Include(r => r.Vehicle)
                .Include(r => r.Driver)
                .Include(r => r.Route)
                .ToListAsync();

            var response = new ListAllFuelConsumptionsResponse();

            foreach (var r in records)
            {
                response.Consumptions.Add(new FuelConsumptionItem
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    RouteId = r.RouteId,
                    DriverId = r.DriverId,
                    Date = r.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    ActualConsumption = r.ActualConsumption,
                    State = r.State,
                    EstimatedConsumptionPerKm = r.EstimatedConsumptionPerKm,

                    Vehicle = r.Vehicle == null ? null : new VehicleDto
                    {
                        Id = r.Vehicle.Id,
                        PlateNumber = r.Vehicle.PlateNumber,
                        Model = r.Vehicle.Model,
                        MachineryType = r.Vehicle.MachineryType,
                        IsOperational = r.Vehicle.IsOperational,
                        FuelCapacity = r.Vehicle.FuelCapacity,
                        RegisteredAt = r.Vehicle.RegisteredAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        FuelConsumptionByKm = r.Vehicle.FuelConsumptionByKm
                    },


                    Driver = new DriverDto
                    {
                        Id = r.Driver.Id,
                        Name = r.Driver.Name,
                        IdentificationNumber = r.Driver.IdentificationNumber,
                        Available = r.Driver.Available,
                        MachineryType = r.Driver.MachineryType
                    },
                    Route = new RouteDto
                    {
                        Id = r.Route.Id,
                        Name = r.Route.Name,
                        Origin = r.Route.Origin,
                        Destiny = r.Route.Destiny,
                        DistanceKm = r.Route.DistanceKm
                    
                    }
                });
            }

            return response;
        }

        public override async Task<FinalizeTripResponse> FinalizeTrip(FinalizeTripRequest request, ServerCallContext context)
        {
            var trip = await _db.FuelConsumptions.FindAsync(request.Id);
            if (trip == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Viaje no encontrado."));
            }

            // Estado fijo: Finalizado
            trip.State = "FINALIZADO";

            //Cambiar el estado del chofer, avality a true

            var driver = await _db.Drivers.FindAsync(trip.DriverId);
            if (driver != null)
            {
                driver.Available = true;
            }
            
            await _db.SaveChangesAsync();
            

            return new FinalizeTripResponse { Status = "OK" };
        }
    }
    }
