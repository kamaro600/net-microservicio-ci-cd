FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5065
EXPOSE 5066

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto
COPY ["UniversityManagement.NotificationService/UniversityManagement.NotificationService.csproj", "UniversityManagement.NotificationService/"]

# Restaurar dependencias NuGet
RUN dotnet restore "UniversityManagement.NotificationService/UniversityManagement.NotificationService.csproj"

# Copiar el código fuente y compilar
COPY . .
WORKDIR "/src/UniversityManagement.NotificationService"
RUN dotnet build "UniversityManagement.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UniversityManagement.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5065

ENTRYPOINT ["dotnet", "UniversityManagement.NotificationService.dll"]