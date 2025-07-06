
using Microsoft.EntityFrameworkCore;
using Grpc.AspNetCore.Server;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql;
using RouteService.Services;
using RouteService.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para HTTP/2 sin TLS en puerto 5184
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5184, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Agregar servicios al contenedor
builder.Services.AddGrpc();

// Registrar RouteDbContext con la cadena de conexión desde appsettings.json
builder.Services.AddDbContext<RouteDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Registrar el servicio gRPC de rutas
builder.Services.AddScoped<RouteServiceImpl>();

var app = builder.Build();

// Mapear el servicio gRPC
app.MapGrpcService<RouteServiceImpl>();

// Endpoint para mostrar JSON simple para pruebas
app.MapGet("/", () => Results.Json(new { message = "Servicio de Rutas gRPC activo." }));

app.Run();
