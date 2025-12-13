# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["UniversityManagement.AuthService/UniversityManagement.AuthService.csproj", "UniversityManagement.AuthService/"]
COPY ["UniversityManagement.Domain/UniversityManagement.Domain.csproj", "UniversityManagement.Domain/"]
COPY ["UniversityManagement.Infrastructure/UniversityManagement.Infrastructure.csproj", "UniversityManagement.Infrastructure/"]
COPY ["UniversityManagement.Application/UniversityManagement.Application.csproj", "UniversityManagement.Application/"]
RUN dotnet restore "UniversityManagement.AuthService/UniversityManagement.AuthService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/UniversityManagement.AuthService"
RUN dotnet build "UniversityManagement.AuthService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UniversityManagement.AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5063
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UniversityManagement.AuthService.dll"]