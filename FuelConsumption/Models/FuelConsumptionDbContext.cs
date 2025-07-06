using DriverService.Models;
using Microsoft.EntityFrameworkCore;
using RouteService.Models;
using VehicleService.Models;

namespace FuelConsumption.Models
{
    public class FuelConsumptionDbContext : DbContext
    {
        public FuelConsumptionDbContext(DbContextOptions<FuelConsumptionDbContext> options)
            : base(options) { }

        public DbSet<FuelConsumption> FuelConsumptions => Set<FuelConsumption>();

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    }
}
