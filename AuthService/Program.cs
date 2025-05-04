using authentication.Models;
using authentication.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; 
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n del DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 36)))
);

// Agregar el servicio gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// Mapear el servicio gRPC
app.MapGrpcService<AuthServiceImpl>();

// Endpoint de prueba para comprobar que el servicio est� corriendo
app.MapGet("/", () => "Servicio de Autenticaci�n gRPC activo.");

app.Run();

