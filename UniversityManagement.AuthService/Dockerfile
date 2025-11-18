FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5063
EXPOSE 5064

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["UniversityManagement.AuthService/UniversityManagement.AuthService.csproj", "UniversityManagement.AuthService/"]

# Restaurar dependencias NuGet
RUN dotnet restore "UniversityManagement.AuthService/UniversityManagement.AuthService.csproj"

# Copiar el código fuente y compilar
COPY . .
WORKDIR "/src/UniversityManagement.AuthService"
RUN dotnet build "UniversityManagement.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UniversityManagement.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5063

ENTRYPOINT ["dotnet", "UniversityManagement.AuthService.dll"]