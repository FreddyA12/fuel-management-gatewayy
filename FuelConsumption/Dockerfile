# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los archivos del proyecto FuelConsumption (relativo al contexto FuelConsumption)
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el proyecto FuelConsumption
COPY . ./

RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5185
ENV ASPNETCORE_URLS=http://+:5185
ENTRYPOINT ["dotnet", "FuelConsumption.dll"]
