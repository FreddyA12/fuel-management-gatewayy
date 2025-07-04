using Microsoft.EntityFrameworkCore;

namespace FuelConsumption.Models
{
    public class FuelConsumptionDbContext : DbContext
    {
        public FuelConsumptionDbContext(DbContextOptions<FuelConsumptionDbContext> options)
            : base(options) { }

        public DbSet<FuelConsumption> FuelConsumptions => Set<FuelConsumption>();
    }
}
