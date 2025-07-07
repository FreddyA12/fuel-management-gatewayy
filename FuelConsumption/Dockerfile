# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todo el repo (ya lo hace docker-compose)
COPY . .

# Cambia al proyecto específico
WORKDIR /src/FuelConsumption

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out . 
EXPOSE 5185
ENV ASPNETCORE_URLS=http://+:5185
ENTRYPOINT ["dotnet", "FuelService.dll"]
