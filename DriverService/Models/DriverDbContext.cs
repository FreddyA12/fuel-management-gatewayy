using Microsoft.EntityFrameworkCore;

namespace DriverService.Models
{
    public class DriverDbContext : DbContext
    {

        public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options) { }

        public DbSet<Driver> Drivers => Set<Driver>();

    }
}
