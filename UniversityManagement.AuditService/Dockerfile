FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5066

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto
COPY ["UniversityManagement.AuditService/UniversityManagement.AuditService.csproj", "UniversityManagement.AuditService/"]

# Restaurar dependencias NuGet
RUN dotnet restore "UniversityManagement.AuditService/UniversityManagement.AuditService.csproj"

# Copiar el código fuente y compilar
COPY . .
WORKDIR "/src/UniversityManagement.AuditService"
RUN dotnet build "UniversityManagement.AuditService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UniversityManagement.AuditService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5066

ENTRYPOINT ["dotnet", "UniversityManagement.AuditService.dll"]