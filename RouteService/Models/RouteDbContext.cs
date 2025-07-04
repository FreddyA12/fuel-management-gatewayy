using Microsoft.EntityFrameworkCore;

namespace VehicleService.Models
{
    public class RouteDbContext : DbContext
    {
        public RouteDbContext(DbContextOptions<RouteDbContext> options) : base(options) { }

        public DbSet<Route> Routes => Set<Route>();
    }
}
