using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UniversityManagement.Application.Services;
using UniversityManagement.Application.Ports.In;
using UniversityManagement.Infrastructure.Data;
using UniversityManagement.Infrastructure;
// using UniversityManagement.Infrastructure.HealthChecks; // Deshabilitado - no usamos Kafka
using UniversityManagement.WebApi.Middleware;

// Configurar comportamiento de DateTime para PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel - Railway usa variable PORT, local usa 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port)); // Puerto dedicado para WebApi - escucha en todas las interfaces
});

// Add services to the container.
builder.Services.AddControllers();

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Universidad Management API",
        Version = "v1",
        Description = "API para gestión universitaria - Clean Architecture Implementation",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Universidad Management System",
            Email = "admin@universidad.edu"
        }
    });
    
    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configurar base de datos
builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention()); // Usar snake_case para PostgreSQL

// Registrar servicios de Infrastructure usando el extension method
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar Health Checks - Solo PostgreSQL ya que usamos microservicios HTTP
// Temporalmente deshabilitado para Railway - habilitar cuando las variables estén configuradas
// builder.Services.AddHealthChecks()
//     .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Registrar casos de uso de Application
builder.Services.AddScoped<IStudentUseCase, StudentUseCase>();
builder.Services.AddScoped<ICareerUseCase, CareerUseCase>();
builder.Services.AddScoped<IFacultyUseCase, FacultyUseCase>();
builder.Services.AddScoped<IProfessorUseCase, ProfessorUseCase>();
builder.Services.AddScoped<IEnrollmentUseCase, EnrollmentUseCase>();

builder.Services.AddHttpContextAccessor();

// Configurar CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware de manejo de excepciones (debe ser el primero para capturar todas las excepciones)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    // Habilitar Swagger UI en desarrollo y producción (para pruebas en containers)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Universidad Management API v1");
        c.RoutePrefix = "swagger"; // Swagger UI será accesible en /swagger
        c.DocumentTitle = "Universidad Management API";
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configurar Health Check endpoints
// app.MapHealthChecks("/health"); // Deshabilitado temporalmente

app.MapControllers();

app.Run();
