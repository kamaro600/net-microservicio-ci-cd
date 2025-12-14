# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["UniversityManagement.WebApi/UniversityManagement.WebApi.csproj", "UniversityManagement.WebApi/"]
COPY ["UniversityManagement.Domain/UniversityManagement.Domain.csproj", "UniversityManagement.Domain/"]
COPY ["UniversityManagement.Infrastructure/UniversityManagement.Infrastructure.csproj", "UniversityManagement.Infrastructure/"]
COPY ["UniversityManagement.Application/UniversityManagement.Application.csproj", "UniversityManagement.Application/"]
RUN dotnet restore "UniversityManagement.WebApi/UniversityManagement.WebApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/UniversityManagement.WebApi"
RUN dotnet build "UniversityManagement.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UniversityManagement.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5000

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UniversityManagement.WebApi.dll"]