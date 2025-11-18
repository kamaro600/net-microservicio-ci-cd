# 🎓 Sistema Universidad - Arquitectura de Microservicios

> Sistema de gestión universitaria implementado con .NET 9, Clean Architecture, Microservicios y Docker.

## 🚀 Inicio Rápido con Docker

### ⚡ Configuración en 3 pasos

```bash
# 1. Clonar y navegar al proyecto
git clone https://github.com/kamaro600/net-microservicio.git
cd net-clean-arquitecture

# 2. Configurar variables de entorno
# Editar .env con tus configuraciones específicas

# 3. Levantar toda la arquitectura
docker-compose up -d --build
```

🌐 **APIs disponibles:**
- **WebAPI Principal:** `http://localhost:5000/swagger`
- **Auth Service:** `http://localhost:5063/swagger` 
- **Notification Service:** `http://localhost:5065/swagger`
- **Audit Service:** `http://localhost:5066/swagger`

### 📋 Requisitos Previos

- **.NET 9 SDK** o superior
- **Docker Desktop** para Windows
- **PostgreSQL 12+** (externo al proyecto)
- **Visual Studio 2022** o **VS Code** (recomendado)

## �️ Arquitectura de Microservicios


### 📦 Servicios Disponibles

| Servicio | Puerto | Propósito | Estado |
|----------|--------|-----------|--------|
| **WebAPI Principal** | 5000 | CRUD Universidad, Estudiantes, Profesores | ✅ Activo |
| **Auth Service** | 5063 | Autenticación JWT, Login, Registro | ✅ Activo |
| **Notification Service** | 5065 | Email/SMS via RabbitMQ | ✅ Activo |
| **Audit Service** | 5066 | Auditoría via Kafka | ✅ Activo |
| **RabbitMQ** | 5672 | Message Broker para notificaciones | ✅ Activo |
| **Kafka** | 9093 | Event Streaming para auditoría | ✅ Activo |
| **Zookeeper** | 2181 | Coordinación Kafka | ✅ Activo |

## 🏢 Estructura del Proyecto

```
UniversityManagement/
├── 🐳 docker-compose.yml                   # Orquestación de microservicios
├── 🔧 .env                                 # Variables de entorno
├── 📄 Dockerfile                           # Imagen Docker principal
│
├── 🌐 UniversityManagement.WebApi/         # API Principal (Puerto 5000)
│   ├── Controllers/                         # Controladores REST API
│   │   ├── StudentsController.cs           # 👨‍🎓 CRUD Estudiantes
│   │   ├── ProfessorsController.cs         # 👨‍🏫 CRUD Profesores  
│   │   ├── CareersController.cs            # 📚 CRUD Carreras
│   │   └── FacultiesController.cs          # 🏛️ CRUD Facultades
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs  # Manejo global de excepciones
│   └── Program.cs                          # Configuración DI y servicios
│
├── � UniversityManagement.AuthService/    # Servicio de Autenticación (Puerto 5063)
│   ├── Controllers/
│   │   └── AuthController.cs               # Login, Register, JWT
│   ├── Services/
│   │   └── AuthService.cs                  # Lógica autenticación
│   └── Dockerfile                          # Imagen Docker Auth
│
├── 📧 UniversityManagement.NotificationService/ # Servicio Notificaciones (Puerto 5065)
│   ├── Controllers/
│   │   └── NotificationController.cs       # APIs de notificación
│   ├── Services/
│   │   ├── EmailService.cs                 # Envío de emails
│   │   └── RabbitMQConsumer.cs            # Consumer RabbitMQ
│   └── Dockerfile                          # Imagen Docker Notifications
│
├── 📊 UniversityManagement.AuditService/   # Servicio de Auditoría (Puerto 5066)
│   ├── Controllers/
│   │   └── AuditController.cs              # APIs de auditoría
│   ├── Services/
│   │   ├── AuditService.cs                 # Lógica auditoría
│   │   └── KafkaConsumer.cs               # Consumer Kafka
│   └── Dockerfile                          # Imagen Docker Audit
│
├── 📱 UniversityManagement.Application/     # Capa de Aplicación Compartida
│   ├── Services/                           # Casos de uso (Use Cases)
│   ├── DTOs/                               # Objetos de transferencia
│   └── Mappers/                            # Conversión Entity ↔ DTO
│
├── � UniversityManagement.Domain/          # Capa de Dominio (Core)
│   ├── Models/                             # Entidades de dominio
│   ├── ValueObjects/                       # Objetos de valor
│   ├── Services/                           # Servicios de dominio
│   └── Repositories/                       # Interfaces de repositorios
│
└── 🔧 UniversityManagement.Infrastructure/  # Capa de Infraestructura
    ├── Data/                               # Contexto Entity Framework
    ├── Persistence/Repositories/           # Implementaciones repositorios
    └── Adapters/Out/                       # Adaptadores servicios externos
```

## 🛠️ Stack Tecnológico

### 🔧 Backend
- **.NET 9** - Framework principal
- **ASP.NET Core** - Web API y Microservicios
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Base de datos (externa)
- **Swagger/OpenAPI** - Documentación API

### 🏗️ Infraestructura
- **Docker & Docker Compose** - Contenedorización y orquestación
- **RabbitMQ** - Message Broker para notificaciones
- **Apache Kafka** - Event Streaming para auditoría
- **Zookeeper** - Coordinación de servicios Kafka

### � Seguridad & Comunicación
- **JWT (JSON Web Tokens)** - Autenticación y autorización
- **HTTPS/HTTP** - Protocolos de comunicación
- **host.docker.internal** - Conectividad container-to-host

## 🐳 Configuración Docker

### 📝 Variables de Entorno (.env)

```bash
# Configuración de Base de Datos Externa
EXTERNAL_DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=UniversidadBD;Username=admin;Password=admin123

# JWT Configuration
JWT_SECRET_KEY=UniversityManagement_JWT_Secret_Key_2024_Very_Long_Secret_Key_For_Security

# RabbitMQ Configuration
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# SMTP Configuration (Mailtrap example)
SMTP_HOST=sandbox.smtp.mailtrap.io
SMTP_PORT=587
SMTP_USERNAME=your_username
SMTP_PASSWORD=your_password
SMTP_FROM_EMAIL=noreply@university.com

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

### 🚀 Comandos Docker

```bash
# Construir y levantar todos los servicios
docker-compose up -d --build

# Ver estado de contenedores
docker-compose ps

# Ver logs de un servicio específico
docker-compose logs -f authservice

# Parar todos los servicios
docker-compose down

# Limpiar todo (contenedores, redes, volúmenes)
docker-compose down -v --remove-orphans
```

## �💾 Base de Datos

### 🐘 PostgreSQL Setup (Externa)

La aplicación se conecta a una base de datos PostgreSQL externa usando `host.docker.internal`:

```sql
-- Crear base de datos
CREATE DATABASE UniversidadBD;

-- Crear usuario admin
CREATE USER admin WITH ENCRYPTED PASSWORD 'admin123';
GRANT ALL PRIVILEGES ON DATABASE UniversidadBD TO admin;

-- Ejecutar script completo
\i database-schema.sql
```

### � Conectividad desde Docker

```bash
# Verificar conectividad desde contenedor
docker run --rm alpine nc -zv host.docker.internal 5432

# Expected output: host.docker.internal (192.168.65.254:5432) open
```

## 🔍 APIs y Testing

### 🌐 Endpoints Principales

#### WebAPI Principal (Puerto 5000)
```bash
# Swagger UI
http://localhost:5000/swagger

# Health Check
curl http://localhost:5000/health

# Listar estudiantes
curl http://localhost:5000/api/students

# Crear estudiante
curl -X POST "http://localhost:5000/api/students" \
     -H "Content-Type: application/json" \
     -d '{
       "nombre": "Juan",
       "apellido": "Pérez", 
       "dni": "12345678",
       "email": "juan@email.com",
       "fechaNacimiento": "1995-05-15"
     }'
```

#### Auth Service (Puerto 5063)
```bash
# Swagger UI
http://localhost:5063/swagger

# Health Check
curl http://localhost:5063/api/auth/health

# Login
curl -X POST "http://localhost:5063/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "user@example.com",
       "password": "password123"
     }'
```

#### Notification Service (Puerto 5065)
```bash
# Swagger UI
http://localhost:5065/swagger

# Health Check
curl http://localhost:5065/api/notifications/health

# Enviar notificación
curl -X POST "http://localhost:5065/api/notifications/send" \
     -H "Content-Type: application/json" \
     -d '{
       "to": "user@example.com",
       "subject": "Test",
       "body": "Mensaje de prueba"
     }'
```

#### Audit Service (Puerto 5066)
```bash
# Swagger UI
http://localhost:5066/swagger

# Health Check
curl http://localhost:5066/api/audit/health

# Consultar auditoría
curl http://localhost:5066/api/audit/logs
```

## 🔧 Desarrollo Local

### 🏃‍♂️ Ejecución sin Docker

```bash
# 1. Configurar PostgreSQL local
# 2. Actualizar connection strings en appsettings.json

# 3. Ejecutar servicios individualmente
cd UniversityManagement.WebApi
dotnet run --urls "http://localhost:5000"

cd ../UniversityManagement.AuthService  
dotnet run --urls "http://localhost:5063"

cd ../UniversityManagement.NotificationService
dotnet run --urls "http://localhost:5065"

cd ../UniversityManagement.AuditService
dotnet run --urls "http://localhost:5066"
```

## 🚨 Troubleshooting

### 🐳 Problemas Comunes Docker

```bash
# Puerto ocupado
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Limpiar Docker
docker system prune -a -f

# Verificar conectividad de red
docker network ls
docker network inspect university-management_university-network
```

### 📡 Problemas de Conectividad

```bash
# Test conectividad PostgreSQL desde contenedor
docker run --rm --network university-management_university-network alpine nc -zv host.docker.internal 5432

# Test conectividad entre servicios
docker exec -it university-webapi curl http://university-authservice:5063/api/auth/health
```

<div align="center">

**⭐ Si este proyecto te resultó útil, considera darle una estrella ⭐**

![.NET](https://img.shields.io/badge/.NET-9.0-blue?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Ready-blue?logo=docker)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-12+-blue?logo=postgresql)
![License](https://img.shields.io/badge/License-MIT-green)

</div>
