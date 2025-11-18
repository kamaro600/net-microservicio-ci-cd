using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniversityManagement.Domain.Repositories;
using UniversityManagement.Domain.Services.Interfaces;
using UniversityManagement.Infrastructure.Data;
using UniversityManagement.Infrastructure.Repositories;
using UniversityManagement.Infrastructure.Mappers;
using UniversityManagement.Domain.Services;
using UniversityManagement.Application.Ports.Out;
using UniversityManagement.Infrastructure.Adapters.Out;

namespace UniversityManagement.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de Infrastructure
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar Entity Framework
        services.AddDbContext<UniversityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Configurar HttpClients para comunicación con microservicios
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        // Registrar handler para propagación de JWT tokens
        services.AddTransient<UniversityManagement.Infrastructure.Http.JwtTokenPropagationHandler>();
        
        // Configurar HttpClient para NotificationService con JWT token propagation
        services.AddHttpClient<IMessagePublisherPort, HttpNotificationPublisherAdapter>()
            .AddHttpMessageHandler<UniversityManagement.Infrastructure.Http.JwtTokenPropagationHandler>();
        
        // Configurar HttpClient para AuditService con JWT token propagation
        services.AddHttpClient<IAuditPublisherPort, HttpAuditPublisherAdapter>()
            .AddHttpMessageHandler<UniversityManagement.Infrastructure.Http.JwtTokenPropagationHandler>();
        


        // Registrar repositorios (Implementaciones de Domain Interfaces)
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICareerRepository, CareerRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IProfessorRepository, ProfessorRepository>();
        services.AddScoped<IStudentCareerRepository, StudentCareerRepository>();

        // Registrar mappers
        services.AddScoped<StudentCareerMapper>();

        // Registrar servicios de dominio
        services.AddScoped<IStudentDomainService, StudentDomainService>();
        services.AddScoped<IProfessorDomainService, ProfessorDomainService>();        

        // Registrar event handlers de dominio
        services.AddScoped<UniversityManagement.Application.Events.StudentEnrolledEventHandler>();
        services.AddScoped<UniversityManagement.Application.Events.StudentUnenrolledEventHandler>();

        return services;
    }
}