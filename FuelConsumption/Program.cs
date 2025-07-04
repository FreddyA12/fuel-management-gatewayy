using Microsoft.EntityFrameworkCore;
using Grpc.AspNetCore.Server;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql;
using FuelConsumption.Models;
using FuelConsumption.Services;


var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para HTTP/2 sin TLS en puerto 5185 (cambia si ya usas ese)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5185, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Agregar servicios al contenedor
builder.Services.AddGrpc();

// Registrar FuelConsumptionDbContext con la cadena de conexión
builder.Services.AddDbContext<FuelConsumptionDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Registrar la implementación gRPC de FuelConsumptionService
builder.Services.AddScoped<FuelConsumptionServiceImpl>();

var app = builder.Build();

// Mapear el servicio gRPC
app.MapGrpcService<FuelConsumptionServiceImpl>();

// Endpoint simple para pruebas HTTP
app.MapGet("/", () => Results.Json(new { message = "Servicio de Consumo de Combustible gRPC activo." }));

app.Run();
