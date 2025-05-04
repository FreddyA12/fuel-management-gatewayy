using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace VehicleService.Models
{
    public class VehicleDbContext : DbContext
    {
        public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    }
}
