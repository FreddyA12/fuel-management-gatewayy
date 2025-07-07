using Authentication.Grpc;
using FuelService.Grpc;
using Serilog;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var builder = WebApplication.CreateBuilder(args);
// Configurar CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("*")
               .AllowAnyMethod() 
               .AllowAnyHeader(); 
    });
});

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, delay, attempt, context) =>
            {
                Console.WriteLine($"🔁 Retry {attempt} after {delay.TotalSeconds} seconds: {outcome.Exception?.Message}");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromSeconds(10),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine($" Circuit broken for {timespan.TotalSeconds}s: {outcome.Exception?.Message}");
            },
            onReset: () => Console.WriteLine(" Circuit reset"),
            onHalfOpen: () => Console.WriteLine(" Circuit in half-open state")
        );
}

//Logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() 
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar cliente gRPC para AuthService
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri("http://authservice:5123");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());


// Configurar cliente gRPC para VehicleService
builder.Services.AddGrpcClient<VehicleService.VehicleService.VehicleServiceClient>(o =>
{
    o.Address = new Uri("http://vehicleservice:5183");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Configurar cliente gRPC para RouteService
builder.Services.AddGrpcClient<RouteService.Grpc.RouteService.RouteServiceClient>(o =>
{
    o.Address = new Uri("http://routeservice:5184");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Configurar cliente gRPC para FuelService
builder.Services.AddGrpcClient<FuelService.Grpc.FuelConsumptionService.FuelConsumptionServiceClient>(o =>
{
    o.Address = new Uri("http://fuelservice:5185");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Cliente gRPC para DriverService (HTTPS)
builder.Services.AddGrpcClient<DriverService.DriverService.DriverServiceClient>(o =>
{
    o.Address = new Uri("http://driverservice:7158");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Cliente gRPC para DriverService (HTTPS)
builder.Services.AddGrpcClient<DriverService.DriverService.DriverServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7158");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();