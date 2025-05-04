using VehicleService.Models;
using Microsoft.EntityFrameworkCore;
using Grpc.AspNetCore.Server;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddGrpc();

// Registrar VehicleDbContext con la cadena de conexi�n desde el archivo appsettings.json
builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Agregar el servicio del microservicio
builder.Services.AddScoped<VehicleService.Services.VehicleServiceImpl>();

var app = builder.Build();

// Configurar la canalizaci�n de la solicitud HTTP
app.MapGrpcService<VehicleService.Services.VehicleServiceImpl>();

// Endpoint para mostrar un JSON al acceder v�a navegador
app.MapGet("/", () => Results.Json(new { message = "Servicio de Veh�culos gRPC activo." }));

// Iniciar el servidor
app.Run();
