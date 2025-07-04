using authentication.Models;
using authentication.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para HTTP/2 sin TLS en puerto 5123 (o el que uses)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5123, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Configuración del DbContext para MySQL
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

// Endpoint de prueba para ver si el servicio está corriendo
app.MapGet("/", () => "Servicio de Autenticación gRPC activo.");

app.Run();
