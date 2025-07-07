# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar el proyecto completo y restaurar dependencias
COPY . . 
RUN dotnet restore

# Publicar la app en modo Release
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar desde la etapa de build los archivos publicados
COPY --from=build /app/out .

# Exponer puertos HTTP y HTTPS
EXPOSE 80
EXPOSE 443

# Configurar URLs para escuchar en todos los interfaces en puertos 80 y 443
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Comando para iniciar la app
ENTRYPOINT ["dotnet", "Gateway.dll"]
