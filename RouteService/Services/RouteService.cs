using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using RouteService.Grpc;
using static RouteService.Grpc.RouteService;
using VehicleService.Models;

namespace RouteService.Services
{
    public class RouteServiceImpl : RouteServiceBase
    {
        private readonly RouteDbContext _db;

        public RouteServiceImpl(RouteDbContext db)
        {
            _db = db;
        }

        public override async Task<RegisterRouteResponse> Register(RegisterRouteRequest request, ServerCallContext context)
        {
            var route = new VehicleService.Models.Route
            {
                Name = request.Name,
                Origin = request.Origin,
                Destiny = request.Destiny,
                DistanceKm = request.DistanceKm,
                EstimatedConsumptionPerKm = request.EstConsumptionPerKm
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync();

            return new RegisterRouteResponse { Status = "OK" };
        }

        public override async Task<GetRouteResponse> GetById(GetRouteRequest request, ServerCallContext context)
        {
            var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == request.Id);

            if (route == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Ruta no encontrada"));

            return new GetRouteResponse
            {
                Id = route.Id,
                Name = route.Name,
                Origin = route.Origin,
                Destiny = route.Destiny,
                DistanceKm = route.DistanceKm,
                EstConsumptionPerKm = route.EstimatedConsumptionPerKm
            };
        }

        public override async Task<UpdateRouteResponse> Update(UpdateRouteRequest request, ServerCallContext context)
        {
            var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == request.Id);

            if (route == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Ruta no encontrada"));

            route.Name = request.Name;
            route.Origin = request.Origin;
            route.Destiny = request.Destiny;
            route.DistanceKm = request.DistanceKm;
            route.EstimatedConsumptionPerKm = request.EstConsumptionPerKm;

            await _db.SaveChangesAsync();

            return new UpdateRouteResponse { Status = "Updated" };
        }

        public override async Task<DeleteRouteResponse> Delete(DeleteRouteRequest request, ServerCallContext context)
        {
            var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == request.Id);

            if (route == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Ruta no encontrada"));

            _db.Routes.Remove(route);
            await _db.SaveChangesAsync();

            return new DeleteRouteResponse { Status = "Deleted" };
        }

        public override async Task<ListAllRoutesResponse> ListAll(Empty request, ServerCallContext context)
        {
            var routes = await _db.Routes.ToListAsync();

            var response = new ListAllRoutesResponse();
            response.Routes.AddRange(routes.Select(r => new RouteItem
            {
                Id = r.Id,
                Name = r.Name,
                Origin = r.Origin,
                Destiny = r.Destiny,
                DistanceKm = r.DistanceKm,
                EstConsumptionPerKm = r.EstimatedConsumptionPerKm
            }));

            return response;
        }
    }
}
