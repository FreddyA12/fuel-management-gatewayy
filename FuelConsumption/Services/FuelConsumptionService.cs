
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
            var record = new Models.FuelConsumption 
            {
                VehicleId = request.VehicleId,
                RouteId = request.RouteId,
                DriverId = request.DriverId,
                Date = DateTime.Parse(request.Date),
                ActualConsumption = request.ActualConsumption
            };

            _db.FuelConsumptions.Add(record);
            await _db.SaveChangesAsync();

            return new RegisterFuelConsumptionResponse { Status = "OK" };
        }

        public override async Task<ListAllFuelConsumptionsResponse> ListAll(Empty request, ServerCallContext context)
        {
            var records = await _db.FuelConsumptions.ToListAsync();

            var response = new ListAllFuelConsumptionsResponse();
            response.Consumptions.AddRange(records.Select(r => new FuelConsumptionItem
            {
                Id = r.Id,
                VehicleId = r.VehicleId,
                RouteId = r.RouteId,
                DriverId = r.DriverId,
                Date = r.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                ActualConsumption = r.ActualConsumption
            }));

            return response;
        }
    }
}
