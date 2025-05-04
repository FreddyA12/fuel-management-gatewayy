using Authentication.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Configurar cliente gRPC para AuthService
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:44310"); 
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Configurar cliente gRPC para VehicleService
/*builder.Services.AddGrpcClient<VehicleService.VehicleServiceClient>(o =>
{
   o.Address = new Uri("https://localhost:5183"); // Cambia al puerto de tu servicio Vehicle
});*/

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();