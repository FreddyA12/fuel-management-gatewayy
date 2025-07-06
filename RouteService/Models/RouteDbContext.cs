using Microsoft.EntityFrameworkCore;

namespace RouteService.Models
{
    public class RouteDbContext : DbContext
    {
        public RouteDbContext(DbContextOptions<RouteDbContext> options) : base(options) { }

        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    }
}
