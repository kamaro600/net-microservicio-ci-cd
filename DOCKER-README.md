# 🐳 University Management System - Docker Deployment

Este documento describe cómo desplegar el sistema completo de gestión universitaria usando Docker y Docker Compose.

## 🏗️ Arquitectura de Contenedores

### 📊 Servicios de Infraestructura
- **PostgreSQL**: Base de datos EXTERNA (reutilizando BD de otro proyecto)
- **Kafka** (`kafka`): Sistema de mensajería para auditoría - Puerto 9093  
- **Zookeeper** (`zookeeper`): Coordinación para Kafka - Puerto 2181
- **Kafka UI** (`kafka-ui`): Interfaz web para Kafka - Puerto 8080
- **RabbitMQ** (`rabbitmq`): Cola de mensajes para notificaciones - Puertos 5672, 15672

### 🎓 Servicios de Aplicación
- **University API** (`university-api`): API principal - Puerto 5000
- **Auth Service** (`auth-service`): Servicio de autenticación - Puerto 5063
- **Notification Service** (`notification-service`): Servicio de notificaciones - Puerto 5065  
- **Audit Service** (`audit-service`): Servicio de auditoría - Puerto 5066

## 🚀 Inicio Rápido

### Prerequisitos
- Docker Desktop instalado y corriendo
- PostgreSQL ejecutándose en el host (puerto 5432) con la BD UniversidadBD
- Git para clonar el repositorio
- Al menos 3GB de RAM disponible (menos que antes al no incluir PostgreSQL)
- Puertos 5000, 5063, 5065, 5066, 9093, 2181, 8080, 5672, 15672 disponibles

### 1. Clonar y Configurar
```bash
git clone <repository-url>
cd net-clean-arquitecture
```

### 2. Configurar Variables de Entorno
Edita el archivo `.env` con tus configuraciones:
```bash
# External Database (actualizar con tu BD existente)
EXTERNAL_DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=TuBaseDeDatos;Username=tu_usuario;Password=tu_password

# SMTP (configura con tu proveedor)
SMTP_HOST=sandbox.smtp.mailtrap.io
SMTP_USERNAME=tu_username
SMTP_PASSWORD=tu_password
```

> **⚠️ Importante**: Asegúrate de que tu base de datos PostgreSQL externa esté corriendo y accesible desde `host.docker.internal:5432`

### 3. Iniciar Servicios

**Opción C: Docker Compose Directo**
```bash
# Iniciar todos los servicios
docker-compose up -d

# Ver el estado
docker-compose ps

# Ver logs
docker-compose logs -f
```

### 4. Verificar Despliegue
Una vez iniciados, verifica que los servicios respondan:
- API Principal: http://localhost:5000/health
- Auth Service: http://localhost:5063/health  
- Notification Service: http://localhost:5065/health
- Audit Service: http://localhost:5066/health

## 🌐 URLs de Acceso

### Servicios de Aplicación
| Servicio | URL | Descripción |
|----------|-----|-------------|
| API Principal | http://localhost:5000 | Swagger UI de la API principal |
| Auth Service | http://localhost:5063 | Servicio de autenticación |
| Notification Service | http://localhost:5065 | Servicio de notificaciones |
| Audit Service | http://localhost:5066 | Servicio de auditoría |

### Interfaces de Administración
| Servicio | URL | Credenciales |
|----------|-----|--------------|
| Kafka UI | http://localhost:8080 | Sin credenciales |
| RabbitMQ Management | http://localhost:15672 | guest/guest |

### Bases de Datos
| Servicio | Conexión | Credenciales |
|----------|----------|--------------|
| PostgreSQL (Externa) | Configurada en tu proyecto existente | Según tu configuración |

## 🔧 Comandos Útiles

### Gestión de Servicios
```bash
# Iniciar servicios
docker-compose up -d

# Detener servicios  
docker-compose down

# Reconstruir imágenes
docker-compose build --no-cache

# Ver logs en tiempo real
docker-compose logs -f [servicio]

# Reiniciar un servicio específico
docker-compose restart [servicio]
```

### Troubleshooting
```bash
# Ver estado de todos los contenedores
docker-compose ps

# Inspeccionar un contenedor específico
docker inspect [container_name]

# Ejecutar comandos dentro de un contenedor
docker-compose exec [servicio] bash

# Ver logs de un servicio específico
docker-compose logs [servicio]

# Limpiar volúmenes (⚠️ elimina datos)
docker-compose down -v
```

## 📁 Estructura de Archivos Docker

```
net-clean-arquitecture/
├── docker-compose.yml          # Configuración principal de servicios
├── .env                        # Variables de entorno
├── .dockerignore              # Archivos excluidos del build
├── deploy.ps1                 # Script de despliegue (Windows)
├── deploy.sh                  # Script de despliegue (Linux/Mac)
├── UniversityManagement.WebApi/
│   ├── Dockerfile             # Imagen de la API principal
│   └── appsettings.Production.json
├── UniversityManagement.AuthService/
│   ├── Dockerfile             # Imagen del servicio de auth
│   └── appsettings.Production.json
├── UniversityManagement.NotificationService/
│   ├── Dockerfile             # Imagen del servicio de notificaciones
│   └── appsettings.Production.json
└── UniversityManagement.AuditService/
    ├── Dockerfile             # Imagen del servicio de auditoría  
    └── appsettings.Production.json
```

### Variables de Entorno Sensibles
- Cambia las contraseñas por defecto en producción
- Usa un JWT secret key fuerte y único
- Configura SMTP con credenciales reales
- Considera usar Docker Secrets para información sensible

### Red de Contenedores
Todos los servicios están aislados en la red `university-network`, permitiendo comunicación interna segura.

## 🔍 Monitoreo y Health Checks

Todos los servicios incluyen health checks automáticos:
- **Base de datos**: Verifica conectividad PostgreSQL
- **Mensajería**: Valida Kafka y RabbitMQ  
- **APIs**: Endpoints /health en cada servicio

## 📝 Logs y Debugging

### Ubicación de Logs
- Logs de aplicación: `docker-compose logs [servicio]`
- Logs del sistema: Docker Desktop > Containers

### Niveles de Log
- Desarrollo: Information level
- Producción: Warning level para Microsoft.AspNetCore

## 🔄 Actualizaciones y Mantenimiento

### Actualizar Servicios
```bash
# Reconstruir después de cambios en código
docker-compose build [servicio]
docker-compose up -d [servicio]

# Actualizar todas las imágenes
docker-compose build --no-cache
docker-compose up -d
```

### Backup de Datos
```bash
# Backup de PostgreSQL
docker-compose exec postgres pg_dump -U admin UniversidadBD > backup.sql

# Restaurar PostgreSQL  
docker-compose exec -T postgres psql -U admin UniversidadBD < backup.sql
```

## 🆘 Solución de Problemas Comunes

### Puerto Ocupado
```bash
# Verificar qué proceso usa un puerto
netstat -tulpn | grep [puerto]

# En Windows
netstat -ano | findstr [puerto]
```

### Servicios no Inician
1. Verificar que Docker Desktop esté corriendo
2. Comprobar puertos disponibles  
3. Revisar logs: `docker-compose logs [servicio]`
4. Verificar variables de entorno en `.env`

### Problemas de Conectividad
1. Verificar que todos los servicios estén saludables: `docker-compose ps`
2. Comprobar la red Docker: `docker network inspect net-clean-arquitecture_university-network`
3. Validar configuración de URLs entre servicios

## 📞 Soporte

Para problemas específicos:
1. Revisa los logs: `docker-compose logs`
2. Verifica el estado: `docker-compose ps`  
3. Consulta la documentación de la API en: http://localhost:5000