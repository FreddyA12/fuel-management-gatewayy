using DriverService.Models;
using DriverService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para escuchar HTTP y HTTPS en los puertos correctos
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7158, listenOptions =>
    {
        listenOptions.UseHttps();  // HTTPS en 7158
    });
    options.ListenLocalhost(5098); // HTTP en 5098 opcional
});

// Add services to the container.
builder.Services.AddGrpc();

// Registrar DriverDbContext con cadena de conexin desde appsettings.json
builder.Services.AddDbContext<DriverDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Registrar el servicio gRPC de choferes
builder.Services.AddScoped<DriverServiceImpl>();

var app = builder.Build();

// Mapear servicio gRPC
app.MapGrpcService<DriverServiceImpl>();

// Endpoint informativo para navegador
app.MapGet("/", () => Results.Json(new { message = "Servicio de Choferes gRPC activo." }));

app.Run();
