using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace authentication.Models
{
    public enum Role { Admin, Operador, Supervisor }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        [Column(TypeName = "varchar(20)")]
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; } = Role.Operador;
    }

    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
    }
}
