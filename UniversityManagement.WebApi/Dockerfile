# Dockerfile para UniversityManagement.WebApi
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["UniversityManagement.WebApi/UniversityManagement.WebApi.csproj", "UniversityManagement.WebApi/"]
COPY ["UniversityManagement.Application/UniversityManagement.Application.csproj", "UniversityManagement.Application/"]
COPY ["UniversityManagement.Domain/UniversityManagement.Domain.csproj", "UniversityManagement.Domain/"]
COPY ["UniversityManagement.Infrastructure/UniversityManagement.Infrastructure.csproj", "UniversityManagement.Infrastructure/"]

# Restaurar dependencias NuGet
RUN dotnet restore "UniversityManagement.WebApi/UniversityManagement.WebApi.csproj"

# Copiar el código fuente y compilar
COPY . .
WORKDIR "/src/UniversityManagement.WebApi"
RUN dotnet build "UniversityManagement.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UniversityManagement.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "UniversityManagement.WebApi.dll"]