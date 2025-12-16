# Diagramas de Arquitectura
# Sistema de Gestión Universitaria

---

## 📐 Índice de Diagramas

1. [Diagrama de Arquitectura General](#1-diagrama-de-arquitectura-general)
2. [Diagrama de Despliegue (Deployment)](#2-diagrama-de-despliegue-deployment)
3. [Diagrama de Pipeline CI/CD](#3-diagrama-de-pipeline-cicd)
4. [Diagrama de Comunicación entre Microservicios](#4-diagrama-de-comunicación-entre-microservicios)
5. [Diagrama de Infraestructura Cloud](#5-diagrama-de-infraestructura-cloud)
6. [Diagrama de Flujo de Datos](#6-diagrama-de-flujo-de-datos)

---

## 1. Diagrama de Arquitectura General

```mermaid
graph TB
    subgraph "Cliente"
        Browser[🌐 Navegador Web]
    end

    subgraph "Frontend Layer"
        Angular[📱 Angular 18 SPA<br/>TypeScript + RxJS]
        Nginx[🔀 Nginx<br/>Reverse Proxy]
    end

    subgraph "API Gateway Layer"
        Gateway[🚪 API Gateway<br/>Enrutamiento + Auth]
    end

    subgraph "Microservicios Backend (.NET 9.0)"
        Auth[🔐 Auth Service<br/>Puerto 5063<br/>JWT + OAuth2]
        WebAPI[🎓 WebAPI Service<br/>Puerto 5000<br/>Lógica de Negocio]
        Notification[📧 Notification Service<br/>Puerto 5065<br/>Email + Push]
        Audit[📊 Audit Service<br/>Puerto 5066<br/>Trazabilidad]
    end

    subgraph "Message Brokers"
        RabbitMQ[🐰 RabbitMQ<br/>Mensajería Transaccional]
        Kafka[📨 Apache Kafka<br/>Event Streaming]
    end

    subgraph "Capa de Datos"
        AuthDB[(🗄️ PostgreSQL<br/>auth_db)]
        UniversityDB[(🗄️ PostgreSQL<br/>university_db)]
        AuditDB[(🗄️ PostgreSQL<br/>audit_db)]
    end

    subgraph "Servicios Externos"
        SMTP[📮 SMTP Server<br/>Envío de Emails]
        Firebase[🔥 Firebase<br/>Push Notifications]
    end

    Browser --> Nginx
    Nginx --> Angular
    Angular -->|REST API| Gateway
    
    Gateway --> Auth
    Gateway --> WebAPI
    Gateway --> Notification
    Gateway --> Audit

    Auth --> AuthDB
    WebAPI --> UniversityDB
    Audit --> AuditDB

    WebAPI -->|Publica Eventos| RabbitMQ
    WebAPI -->|Logs de Auditoría| Kafka
    
    RabbitMQ --> Notification
    Kafka --> Audit

    Notification --> SMTP
    Notification --> Firebase

    style Angular fill:#dd0031,color:#fff
    style Auth fill:#512bd4,color:#fff
    style WebAPI fill:#512bd4,color:#fff
    style Notification fill:#512bd4,color:#fff
    style Audit fill:#512bd4,color:#fff
    style RabbitMQ fill:#ff6600,color:#fff
    style Kafka fill:#000000,color:#fff
```

**Descripción:**
- **Frontend**: Angular SPA servida por Nginx
- **Backend**: 4 microservicios .NET independientes
- **Datos**: PostgreSQL separado por servicio (Database per Service)
- **Mensajería**: RabbitMQ para eventos transaccionales, Kafka para auditoría
- **Integración**: API Gateway para enrutamiento centralizado

---

## 2. Diagrama de Despliegue (Deployment)

```mermaid
graph TB
    subgraph "GitHub"
        Repo[📦 Repositorio<br/>kamaro600/net-microservicio-ci-cd]
        Actions[⚙️ GitHub Actions<br/>CI/CD Pipeline]
        Registry[📦 ghcr.io<br/>Container Registry]
    end

    subgraph "Railway Cloud Platform"
        subgraph "Frontend Service"
            FrontendContainer[🐳 Frontend Container<br/>Angular + Nginx<br/>Port 8080/8081]
        end

        subgraph "Auth Service"
            AuthContainer[🐳 Auth Container<br/>.NET 9.0<br/>Port 5063]
        end

        subgraph "WebAPI Service"
            WebAPIContainer[🐳 WebAPI Container<br/>.NET 9.0<br/>Port 5000]
        end

        subgraph "Notification Service"
            NotificationContainer[🐳 Notification Container<br/>.NET 9.0<br/>Port 5065]
        end

        subgraph "Audit Service"
            AuditContainer[🐳 Audit Container<br/>.NET 9.0<br/>Port 5066]
        end

        subgraph "Databases"
            NeonDB[(☁️ Neon PostgreSQL<br/>Serverless)]
        end

        subgraph "Message Brokers"
            RMQ[🐰 RabbitMQ<br/>Managed]
            KFK[📨 Kafka<br/>Managed]
        end
    end

    subgraph "SonarCloud"
        Sonar[🔍 SonarCloud<br/>Code Quality]
    end

    subgraph "DNS"
        Domain[🌐 www.kamaro.online<br/>Custom Domain]
    end

    Repo -->|Push Code| Actions
    Actions -->|Build & Test| Sonar
    Actions -->|Quality Gate ✅| Registry
    Registry -->|Public Images| Actions

    Actions -->|Wait for CI ✅| Railway
    
    Railway -->|Build from Dockerfile| FrontendContainer
    Railway -->|Build from Dockerfile| AuthContainer
    Railway -->|Build from Dockerfile| WebAPIContainer
    Railway -->|Build from Dockerfile| NotificationContainer
    Railway -->|Build from Dockerfile| AuditContainer

    AuthContainer --> NeonDB
    WebAPIContainer --> NeonDB
    AuditContainer --> NeonDB

    WebAPIContainer --> RMQ
    NotificationContainer --> RMQ
    WebAPIContainer --> KFK
    AuditContainer --> KFK

    Domain -->|CNAME| FrontendContainer

    style Repo fill:#24292e,color:#fff
    style Actions fill:#2088ff,color:#fff
    style Registry fill:#2088ff,color:#fff
    style Sonar fill:#cb3032,color:#fff
    style Railway fill:#0b0d0e,color:#fff
```

**Descripción:**
- **Código Fuente**: GitHub repository
- **CI/CD**: GitHub Actions con Quality Gate de SonarCloud
- **Registry**: ghcr.io para imágenes públicas (opcional)
- **Hosting**: Railway con 5 contenedores independientes
- **Base de Datos**: Neon PostgreSQL (managed)
- **Dominio**: www.kamaro.online con SSL automático

---

## 3. Diagrama de Pipeline CI/CD

```mermaid
graph LR
    A[📝 Git Push<br/>main/develop] --> B[🔨 Job 1: Build & Test<br/>dotnet restore<br/>dotnet build<br/>dotnet test]
    
    B --> C[🔍 Job 2: SonarQube Analysis<br/>Code Quality<br/>Coverage<br/>Security]
    
    C --> D{🚦 Quality Gate}
    
    D -->|❌ FAIL| E[🛑 Pipeline Bloqueado<br/>No Deploy]
    D -->|✅ PASS| F[🌐 Job 3: Build Frontend<br/>npm ci<br/>npm run build]
    
    C --> F
    
    F --> G[🐳 Job 4: Build Docker Images<br/>5 servicios en paralelo<br/>auth, webapi, notification<br/>audit, frontend]
    
    G --> H[📦 Push to ghcr.io<br/>Imágenes públicas]
    
    H --> I[🚂 Railway Auto-Deploy<br/>Wait for CI ✅]
    
    I --> J[✅ 5 Servicios Desplegados<br/>www.kamaro.online]

    style A fill:#2ea44f,color:#fff
    style B fill:#0969da,color:#fff
    style C fill:#bf3989,color:#fff
    style D fill:#fb8500,color:#fff
    style E fill:#cf222e,color:#fff
    style F fill:#0969da,color:#fff
    style G fill:#1f6feb,color:#fff
    style H fill:#6e7781,color:#fff
    style I fill:#000000,color:#fff
    style J fill:#2ea44f,color:#fff
```

**Etapas del Pipeline:**

1. **Build & Test**: Compilación y tests unitarios de .NET
2. **SonarQube**: Análisis de calidad (Coverage, Bugs, Vulnerabilities)
3. **Quality Gate**: Checkpoint que bloquea si no cumple estándares
4. **Build Frontend**: Compilación de Angular en modo producción
5. **Docker Build**: Construcción de 5 imágenes Docker en paralelo
6. **Publish**: Subida de imágenes a ghcr.io
7. **Railway Deploy**: Despliegue automático a Railway

**Tiempo Estimado Total:** ~12-15 minutos

---

## 4. Diagrama de Comunicación entre Microservicios

```mermaid
sequenceDiagram
    participant U as 👤 Usuario
    participant F as 📱 Frontend
    participant A as 🔐 Auth Service
    participant W as 🎓 WebAPI
    participant N as 📧 Notification
    participant D as 📊 Audit
    participant MQ as 🐰 RabbitMQ
    participant K as 📨 Kafka
    participant DB as 🗄️ PostgreSQL

    U->>F: 1. Login
    F->>A: 2. POST /api/auth/login
    A->>DB: 3. Verificar credenciales
    DB-->>A: 4. Usuario válido
    A-->>F: 5. JWT Token
    F-->>U: 6. Acceso concedido

    U->>F: 7. Inscribir estudiante en curso
    F->>W: 8. POST /api/enrollments<br/>(JWT Header)
    W->>A: 9. Validar token
    A-->>W: 10. Token válido ✅
    
    W->>DB: 11. Crear inscripción
    DB-->>W: 12. Inscripción creada
    
    W->>MQ: 13. Publish: StudentEnrolled event
    W->>K: 14. Publish: AuditLog event
    
    MQ->>N: 15. Consume: StudentEnrolled
    N->>DB: 16. Guardar notificación
    N-->>U: 17. Enviar email confirmación
    
    K->>D: 18. Consume: AuditLog
    D->>DB: 19. Guardar log de auditoría
    
    W-->>F: 20. Response: 201 Created
    F-->>U: 21. Confirmación en pantalla

    Note over W,MQ: Comunicación Asíncrona<br/>Desacoplada
    Note over W,K: Event Streaming<br/>para Auditoría
```

**Flujo de Comunicación:**

- **Sincrónica (REST)**: Frontend ↔ Servicios (HTTP/HTTPS)
- **Asincrónica (Eventos)**: Servicios ↔ RabbitMQ/Kafka
- **Autenticación**: JWT validado en cada request
- **Desacoplamiento**: Servicios no se conocen entre sí, solo eventos

---

## 5. Diagrama de Infraestructura Cloud

```mermaid
graph TB
    subgraph "Internet"
        Users[👥 Usuarios]
    end

    subgraph "Railway Cloud Infrastructure"
        subgraph "Load Balancer"
            LB[⚖️ Railway Load Balancer<br/>SSL/TLS Termination]
        end

        subgraph "Container Runtime (Kubernetes-like)"
            subgraph "Pod 1"
                C1[🐳 auth-service<br/>Replica 1]
            end
            subgraph "Pod 2"
                C2[🐳 webapi<br/>Replica 1]
            end
            subgraph "Pod 3"
                C3[🐳 notification<br/>Replica 1]
            end
            subgraph "Pod 4"
                C4[🐳 audit<br/>Replica 1]
            end
            subgraph "Pod 5"
                C5[🐳 frontend<br/>Replica 1]
            end
        end

        subgraph "Managed Services"
            NeonDB[(☁️ Neon PostgreSQL<br/>Auto-scaling)]
            RMQ[🐰 RabbitMQ<br/>Managed Queue]
            Kafka[📨 Kafka<br/>Managed Streaming]
        end

        subgraph "Monitoring"
            Logs[📋 Railway Logs<br/>Centralized]
            Metrics[📊 Railway Metrics<br/>CPU/RAM/Network]
        end
    end

    subgraph "External Services"
        SMTP[📮 SMTP Provider]
        Firebase[🔥 Firebase]
    end

    Users -->|HTTPS| LB
    LB --> C1
    LB --> C2
    LB --> C3
    LB --> C4
    LB --> C5

    C1 --> NeonDB
    C2 --> NeonDB
    C4 --> NeonDB

    C2 --> RMQ
    C3 --> RMQ

    C2 --> Kafka
    C4 --> Kafka

    C3 --> SMTP
    C3 --> Firebase

    C1 -.->|Logs| Logs
    C2 -.->|Logs| Logs
    C3 -.->|Logs| Logs
    C4 -.->|Logs| Logs
    C5 -.->|Logs| Logs

    C1 -.->|Metrics| Metrics
    C2 -.->|Metrics| Metrics
    C3 -.->|Metrics| Metrics
    C4 -.->|Metrics| Metrics
    C5 -.->|Metrics| Metrics

    style LB fill:#0078d4,color:#fff
    style NeonDB fill:#00cc88,color:#fff
    style RMQ fill:#ff6600,color:#fff
    style Kafka fill:#000000,color:#fff
    style Logs fill:#6e7781,color:#fff
    style Metrics fill:#2ea44f,color:#fff
```

**Componentes de Infraestructura:**

- **Load Balancer**: Railway gestiona SSL/TLS y enrutamiento
- **Container Runtime**: Similar a Kubernetes, orquestación automática
- **Managed DB**: Neon PostgreSQL con auto-scaling
- **Message Brokers**: RabbitMQ y Kafka gestionados
- **Observability**: Logs centralizados y métricas de infraestructura

---

## 6. Diagrama de Flujo de Datos

```mermaid
graph LR
    subgraph "Input"
        User[👤 Usuario<br/>Acción]
    end

    subgraph "Presentation Layer"
        UI[📱 Angular UI<br/>Componentes + Servicios]
    end

    subgraph "Business Logic"
        API[🎓 WebAPI<br/>Domain Logic<br/>Validaciones]
    end

    subgraph "Data Access"
        EF[📊 Entity Framework Core<br/>ORM]
    end

    subgraph "Persistence"
        DB[(🗄️ PostgreSQL<br/>Datos Transaccionales)]
    end

    subgraph "Event Publishing"
        EventBus[📡 Event Bus<br/>RabbitMQ + Kafka]
    end

    subgraph "Consumers"
        Notification[📧 Notification<br/>Envío Emails]
        Audit[📊 Audit<br/>Trazabilidad]
    end

    subgraph "Output"
        Response[✅ Respuesta<br/>al Usuario]
    end

    User -->|HTTP Request| UI
    UI -->|REST API Call<br/>+ JWT| API
    API -->|LINQ Queries| EF
    EF -->|SQL Queries| DB
    DB -->|Result Set| EF
    EF -->|Entities| API
    
    API -->|Publish Events| EventBus
    EventBus -->|StudentEnrolled| Notification
    EventBus -->|AuditLog| Audit
    
    Notification -->|Email Sent| Response
    Audit -->|Log Saved| Response
    
    API -->|JSON Response| UI
    UI -->|Render| User

    style User fill:#0078d4,color:#fff
    style UI fill:#dd0031,color:#fff
    style API fill:#512bd4,color:#fff
    style EF fill:#512bd4,color:#fff
    style DB fill:#336791,color:#fff
    style EventBus fill:#ff6600,color:#fff
    style Notification fill:#2ea44f,color:#fff
    style Audit fill:#6e7781,color:#fff
    style Response fill:#2ea44f,color:#fff
```

**Capas de la Arquitectura:**

1. **Presentation**: Angular (TypeScript, RxJS, Angular Material)
2. **Business Logic**: WebAPI (.NET 9.0, Domain-Driven Design)
3. **Data Access**: Entity Framework Core (ORM)
4. **Persistence**: PostgreSQL (Relacional)
5. **Event-Driven**: RabbitMQ + Kafka (Mensajería)
6. **Cross-Cutting**: Notification + Audit (Microservicios auxiliares)

---

## 📊 Resumen de Tecnologías por Componente

| Componente | Tecnología | Puerto | Base de Datos |
|------------|------------|--------|---------------|
| **Frontend** | Angular 18 + Nginx | 8080/8081 | - |
| **Auth Service** | .NET 9.0 + JWT | 5063 | PostgreSQL (auth_db) |
| **WebAPI Service** | .NET 9.0 + EF Core | 5000 | PostgreSQL (university_db) |
| **Notification Service** | .NET 9.0 | 5065 | - |
| **Audit Service** | .NET 9.0 | 5066 | PostgreSQL (audit_db) |
| **RabbitMQ** | Managed | 5672 | - |
| **Kafka** | Managed | 9092 | - |

---

## 📡 APIs Expuestas por Servicio

### 🔐 Auth Service (Puerto 5063)

**Base URL:** `/api/auth`

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/login` | Autenticar usuario y generar JWT token | ❌ No |
| `POST` | `/register` | Registrar nuevo usuario | ❌ No |
| `POST` | `/validate` | Validar token JWT | ❌ No |
| `POST` | `/refresh` | Refrescar token expirado | ✅ Sí |
| `GET` | `/me` | Obtener información del usuario actual | ✅ Sí |

**Ejemplo Request:**
```bash
# Login
POST https://www.kamaro.online/api/auth/login
{
  "username": "usuario@example.com",
  "password": "password123"
}
```

---

### 🎓 WebAPI Service (Puerto 5000)

**Base URL:** `/api`

#### **Students** (`/api/students`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `GET` | `/{id}` | Obtener estudiante por ID | ✅ Sí |
| `POST` | `/` | Crear nuevo estudiante | ✅ Sí |

#### **Professors** (`/api/professors`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `GET` | `/{id}` | Obtener profesor por ID | ✅ Sí |
| `GET` | `/` | Listar profesores (con filtro opcional) | ✅ Sí |
| `POST` | `/` | Crear nuevo profesor | ✅ Sí |

#### **Careers** (`/api/careers`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `GET` | `/{id}` | Obtener carrera por ID | ❌ No |
| `GET` | `/` | Listar carreras (con filtro opcional) | ❌ No |
| `POST` | `/` | Crear nueva carrera | ❌ No |

#### **Faculties** (`/api/faculties`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `GET` | `/{id}` | Obtener facultad por ID | ❌ No |
| `GET` | `/` | Listar facultades (con filtro opcional) | ❌ No |
| `POST` | `/` | Crear nueva facultad | ❌ No |

#### **Enrollment** (`/api/enrollment`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/enroll` | Matricular estudiante en carrera | ✅ Sí |
| `POST` | `/unenroll` | Desmatricular estudiante de carrera | ✅ Sí |
| `GET` | `/student/{studentId}` | Obtener matrículas de estudiante | ✅ Sí |

#### **Health** (`/api/health`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `GET` | `/` | Health check del servicio | ❌ No |

**Ejemplo Request:**
```bash
# Matricular estudiante
POST https://www.kamaro.online/api/enrollment/enroll
Authorization: Bearer <JWT_TOKEN>
{
  "studentId": 123,
  "careerId": 456
}
```

---

### 📧 Notification Service (Puerto 5065)

**Base URL:** `/api/notifications`

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/enrollment` | Enviar notificación de matrícula | ✅ Sí |
| `POST` | `/general` | Enviar notificación general | ✅ Sí |
| `GET` | `/history/{userId}` | Obtener historial de notificaciones | ✅ Sí |

**Ejemplo Request:**
```bash
# Notificación de matrícula
POST https://www.kamaro.online/api/notifications/enrollment
Authorization: Bearer <JWT_TOKEN>
{
  "studentEmail": "student@example.com",
  "studentName": "Juan Pérez",
  "careerName": "Ingeniería de Sistemas",
  "messageId": "msg-12345"
}
```

---

### 📊 Audit Service (Puerto 5066)

**Base URL:** `/api/audit`

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/events` | Registrar evento de auditoría | ✅ Sí |
| `GET` | `/events` | Listar eventos de auditoría (con filtros) | ✅ Sí |
| `GET` | `/events/{id}` | Obtener evento por ID | ✅ Sí |
| `GET` | `/events/entity/{entityName}/{entityId}` | Buscar eventos por entidad | ✅ Sí |

**Ejemplo Request:**
```bash
# Registrar evento de auditoría
POST https://www.kamaro.online/api/audit/events
Authorization: Bearer <JWT_TOKEN>
{
  "eventType": "StudentEnrolled",
  "entityName": "Enrollment",
  "entityId": "123",
  "userId": "user-456",
  "action": "CREATE",
  "timestamp": "2025-12-15T10:30:00Z"
}
```

---

## 🔑 Autenticación y Autorización

### Flujo de Autenticación JWT

```mermaid
sequenceDiagram
    participant C as Cliente
    participant A as Auth Service
    participant W as WebAPI/Otros

    C->>A: POST /api/auth/login<br/>{username, password}
    A-->>C: 200 OK<br/>{token, refreshToken}
    
    Note over C: Cliente guarda token<br/>en localStorage/cookie
    
    C->>W: GET /api/students/123<br/>Authorization: Bearer {token}
    W->>A: POST /api/auth/validate<br/>{token}
    A-->>W: {isValid: true, userId: "..."}
    W-->>C: 200 OK<br/>{student data}
    
    Note over C: Si token expira...
    
    C->>A: POST /api/auth/refresh<br/>{refreshToken}
    A-->>C: 200 OK<br/>{newToken, newRefreshToken}
```

### Headers Requeridos

**Para endpoints con autenticación (✅):**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Para endpoints públicos (❌):**
```http
Content-Type: application/json
```

---

## 📝 Códigos de Respuesta HTTP

| Código | Significado | Cuándo se usa |
|--------|-------------|---------------|
| `200 OK` | Operación exitosa | GET, PUT, PATCH exitosos |
| `201 Created` | Recurso creado | POST exitoso |
| `400 Bad Request` | Datos inválidos | Validación fallida |
| `401 Unauthorized` | No autenticado | Token ausente o inválido |
| `403 Forbidden` | Sin permisos | Usuario no autorizado |
| `404 Not Found` | Recurso no existe | ID no encontrado |
| `500 Internal Server Error` | Error del servidor | Excepción no controlada |

---

## DevOps y Despliegue

### Estrategia de CI/CD

El proyecto implementa un pipeline completo de CI/CD utilizando **GitHub Actions** con 5 jobs principales que garantizan la calidad del código antes del despliegue.

#### Pipeline de GitHub Actions

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  # Job 1: Build y Test de .NET
  build-and-test:
    - dotnet restore
    - dotnet build --configuration Release
    - dotnet test --no-build --verbosity normal
    
  # Job 2: Análisis estático con SonarCloud
  sonarqube:
    - SonarCloud Scan (Coverage, Bugs, Vulnerabilities)
    - Quality Gate Check (BLOQUEA si falla)
    
  # Job 3: Build de Frontend Angular
  frontend:
    - npm ci
    - npm run build --prod
    - npm run test (si aplica)
    
  # Job 4: Build y Push de imágenes Docker
  docker-build:
    - Build de 5 imágenes (auth, webapi, notification, audit, frontend)
    - Tag con SHA del commit
    - Push a ghcr.io (GitHub Container Registry)
    
  # Job 5: Comentario en Railway
  railway-comment:
    - Genera comentario con status del deployment
```

#### Herramientas de CI/CD

| Herramienta | Propósito | Configuración |
|-------------|-----------|---------------|
| **GitHub Actions** | Orquestación de pipeline | `.github/workflows/ci-cd.yml` |
| **SonarCloud** | Análisis de calidad de código | `sonar-project.properties` |
| **Docker** | Containerización | `Dockerfile.*` (multi-stage) |
| **ghcr.io** | Registry de imágenes públicas | GitHub Container Registry |
| **Railway** | Plataforma de despliegue | `railway.json` |

#### Quality Gate Enforcement

```mermaid
graph LR
    A[🔨 Build & Test] --> B[🔍 SonarCloud Scan]
    B --> C{🚦 Quality Gate}
    C -->|❌ FAIL| D[🛑 Pipeline Bloqueado<br/>No se despliega]
    C -->|✅ PASS| E[🐳 Docker Build]
    E --> F[📦 Push to ghcr.io]
    F --> G[🚂 Railway Deploy]
    
    style C fill:#fb8500,color:#fff
    style D fill:#cf222e,color:#fff
    style G fill:#2ea44f,color:#fff
```

**Reglas del Quality Gate:**
- ✅ Coverage mínimo configurado en SonarCloud
- ✅ Bugs = 0 (configurable)
- ✅ Vulnerabilities = 0 (configurable)
- ✅ Code Smells bajo límite (configurable)
- ⚠️ **Pipeline FALLA si Quality Gate retorna ERROR**

---

### Infraestructura y Despliegue

#### Arquitectura de Despliegue en Railway

El proyecto utiliza **Railway** como plataforma PaaS (Platform as a Service) que abstrae la complejidad de la infraestructura subyacente.

```mermaid
graph TB
    subgraph "GitHub Repository"
        Code[📦 Source Code<br/>main branch]
        Actions[⚙️ GitHub Actions<br/>CI Pipeline]
    end

    subgraph "Railway Platform (PaaS)"
        subgraph "Build Process"
            Builder[🏗️ Railway Builder<br/>Docker Build]
        end

        subgraph "Container Runtime"
            Auth[🔐 Auth Container]
            WebAPI[🎓 WebAPI Container]
            Notif[📧 Notification Container]
            Audit[📊 Audit Container]
            Front[📱 Frontend Container]
        end

        subgraph "Managed Services"
            LB[⚖️ Load Balancer<br/>+ SSL/TLS]
            DNS[🌐 Custom Domain<br/>www.kamaro.online]
            Logs[📋 Logs Centralized]
            Metrics[📊 Metrics Dashboard]
        end

        subgraph "External Managed Services"
            NeonDB[(☁️ Neon PostgreSQL<br/>Serverless)]
            RMQ[🐰 RabbitMQ Cloud]
            Kafka[📨 Kafka Cloud]
        end
    end

    Code -->|Git Push| Actions
    Actions -->|Wait for CI ✅| Builder
    Builder -->|Deploy| Auth
    Builder -->|Deploy| WebAPI
    Builder -->|Deploy| Notif
    Builder -->|Deploy| Audit
    Builder -->|Deploy| Front

    Auth --> NeonDB
    WebAPI --> NeonDB
    Audit --> NeonDB

    WebAPI --> RMQ
    Notif --> RMQ

    WebAPI --> Kafka
    Audit --> Kafka

    LB --> Front
    DNS --> LB

    Auth -.->|Logs| Logs
    WebAPI -.->|Logs| Logs
    Notif -.->|Logs| Logs
    Audit -.->|Logs| Logs
    Front -.->|Logs| Logs

    style Builder fill:#0b0d0e,color:#fff
    style LB fill:#0078d4,color:#fff
    style NeonDB fill:#00cc88,color:#fff
```

#### Configuración de Infraestructura

**Railway** gestiona automáticamente:
- ✅ **Networking**: VPC privada, balanceadores de carga, DNS
- ✅ **SSL/TLS**: Certificados Let's Encrypt automáticos
- ✅ **Escalado**: Auto-scaling vertical (CPU/RAM)
- ✅ **Monitoreo**: Logs centralizados, métricas de recursos
- ✅ **Zero-downtime deployments**: Rolling updates


#### Base de Datos (Neon PostgreSQL)

**Características:**
- ✅ **Serverless**: Auto-scaling y auto-suspend
- ✅ **Branching**: Base de datos por branch (dev/staging/prod)
- ✅ **Backups**: Automáticos diarios con retención de 7 días
- ✅ **Connection Pooling**: PgBouncer integrado
- ✅ **Alta disponibilidad**: Réplicas automáticas

**Variables de entorno en Railway:**
```bash
# Auth Service
DATABASE_URL=postgresql://user:pass@host/auth_db

# WebAPI Service
DATABASE_URL=postgresql://user:pass@host/university_db

# Audit Service
DATABASE_URL=postgresql://user:pass@host/audit_db
```

#### Gestión de Secretos

**Railway Environment Variables:**
- 🔒 Almacenamiento cifrado de secretos
- 🔒 Inyección automática en contenedores
- 🔒 No se exponen en logs ni en código fuente
- 🔒 Variables por servicio y por entorno

---

### Ambientes de Despliegue

#### Estrategia de Branching y Ambientes

```mermaid
graph LR
    subgraph "Git Branches"
        Dev[🔧 develop branch]
        Main[🚀 main branch]
    end

    subgraph "Railway Environments"
        DevEnv[🧪 Development<br/>PR Previews]
        ProdEnv[🌐 Production<br/>www.kamaro.online]
    end

    subgraph "Database Branches"
        DevDB[(🗄️ dev_db<br/>Neon Branch)]
        ProdDB[(🗄️ prod_db<br/>Neon Main)]
    end

    Dev -->|PR Preview| DevEnv
    Main -->|Auto Deploy| ProdEnv

    DevEnv --> DevDB
    ProdEnv --> ProdDB

    style Dev fill:#fb8500,color:#fff
    style Main fill:#2ea44f,color:#fff
    style ProdEnv fill:#0078d4,color:#fff
```

#### Configuración de Ambientes

| Ambiente | Branch | Railway | URL | Base de Datos | CI/CD |
|----------|--------|---------|-----|---------------|-------|
| **Development** | `develop` | PR Previews | `<pr-id>.up.railway.app` | Neon Branch (dev) | ✅ Run CI, ⚠️ Sin Quality Gate |
| **Production** | `main` | Production Service | `www.kamaro.online` | Neon Main Branch | ✅ Full CI/CD + Quality Gate |

#### Flujo de Trabajo GitOps

```mermaid
sequenceDiagram
    participant Dev as 👨‍💻 Developer
    participant GH as 📦 GitHub
    participant CI as ⚙️ GitHub Actions
    participant SQ as 🔍 SonarCloud
    participant RW as 🚂 Railway

    Dev->>GH: 1. Push to develop branch
    GH->>CI: 2. Trigger CI Pipeline
    CI->>CI: 3. Build & Test
    CI->>SQ: 4. Code Quality Analysis
    SQ-->>CI: 5. Quality Report (no blocking)
    CI->>RW: 6. Build Docker image
    RW-->>Dev: 7. PR Preview URL

    Note over Dev: Developer revisa PR Preview

    Dev->>GH: 8. Create Pull Request (develop → main)
    GH->>CI: 9. Trigger CI Pipeline (PR)
    CI->>CI: 10. Build & Test
    CI->>SQ: 11. Code Quality Analysis
    SQ-->>CI: 12. Quality Gate Check
    
    alt Quality Gate PASS
        CI-->>GH: 13. ✅ Status Check Pass
        Note over GH: PR ready to merge
        Dev->>GH: 14. Merge PR to main
        GH->>CI: 15. Trigger Production CI/CD
        CI->>CI: 16. Build & Test
        CI->>SQ: 17. Quality Gate (blocking)
        SQ-->>CI: 18. ✅ PASS
        CI->>RW: 19. Deploy to Production
        RW-->>Dev: 20. ✅ Deployed to www.kamaro.online
    else Quality Gate FAIL
        CI-->>GH: 13. ❌ Status Check Fail
        SQ-->>Dev: 14. Quality Issues Report
        Note over Dev: Fix issues before merge
    end
```

#### Estrategia de Despliegue

**1. Despliegue a Producción:**
- Trigger: Merge a `main` branch
- Validación: Quality Gate DEBE pasar
- Estrategia: Rolling update (zero-downtime)
- Rollback: Revert commit o redeploy desde Railway UI
- Monitoreo: Railway Logs + Metrics


---

### Monitoreo y Observabilidad

#### Herramientas de Monitoreo

```mermaid
graph TB
    subgraph "Application Layer"
        App[📱 Microservicios<br/>5 Contenedores]
    end

    subgraph "Logging"
        RWLogs[📋 Railway Logs<br/>Stdout/Stderr]
        LogStream[🔄 Log Streaming<br/>Real-time]
    end

    subgraph "Metrics"
        RWMetrics[📊 Railway Metrics<br/>CPU, RAM, Network]
        Uptime[⏱️ Uptime Monitoring]
    end

    subgraph "Quality"
        SonarDash[🔍 SonarCloud Dashboard<br/>Code Quality]
    end

    subgraph "CI/CD"
        GHActions[⚙️ GitHub Actions<br/>Pipeline Status]
    end

    App -->|Logs| RWLogs
    App -->|Metrics| RWMetrics
    RWLogs --> LogStream
    RWMetrics --> Uptime

    style App fill:#512bd4,color:#fff
    style RWLogs fill:#6e7781,color:#fff
    style RWMetrics fill:#2ea44f,color:#fff
    style SonarDash fill:#cb3032,color:#fff
```

#### Dashboards Disponibles

| Dashboard | URL | Información |
|-----------|-----|-------------|
| **Railway Logs** | `railway.app/project/<id>/logs` | Logs en tiempo real de todos los servicios |
| **Railway Metrics** | `railway.app/project/<id>/metrics` | CPU, RAM, Network, Uptime |
| **SonarCloud** | `sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd` | Quality metrics, coverage, bugs |
| **GitHub Actions** | `github.com/<repo>/actions` | Estado de pipelines, history |
| **Production Site** | `www.kamaro.online` | Health check endpoints |


---


##  Seguridad en la Arquitectura

```mermaid
graph TB
    subgraph "Seguridad de Red"
        HTTPS[ HTTPS/TLS<br/>Let's Encrypt]
        CORS[ CORS Policy<br/>Dominios Permitidos]
    end

    subgraph "Seguridad de Aplicación"
        JWT[ JWT Tokens<br/>OAuth2 + Bearer]
        Auth[ Authorization<br/>RBAC]
        Validation[ Input Validation<br/>Data Sanitization]
    end

    subgraph "Seguridad de Infraestructura"
        Secrets[ Secrets Management<br/>Railway Environment Variables]
        Network[ Network Isolation<br/>Private Networking]
        Container[ Container Security<br/>Non-root Users]
    end

    subgraph "Seguridad de Datos"
        Encryption[ Data Encryption<br/>At Rest + In Transit]
        Backup[ Automated Backups<br/>Neon PostgreSQL]
    end

    HTTPS --> JWT
    CORS --> JWT
    JWT --> Auth
    Auth --> Validation
    
    Secrets --> Container
    Network --> Container
    
    Encryption --> Backup

    style HTTPS fill:#0078d4,color:#fff
    style JWT fill:#512bd4,color:#fff
    style Auth fill:#512bd4,color:#fff
    style Secrets fill:#6e7781,color:#fff
    style Container fill:#1f6feb,color:#fff
    style Encryption fill:#2ea44f,color:#fff
```

**Capas de Seguridad Implementadas:**

- ✅ HTTPS obligatorio con certificados SSL automáticos
- ✅ JWT para autenticación stateless
- ✅ RBAC para autorización basada en roles
- ✅ Contenedores ejecutando como usuarios no privilegiados
- ✅ Secrets gestionados por Railway (no en código)
- ✅ Validación de entrada en todos los endpoints
- ✅ Cifrado de datos en tránsito y reposo

---

## 🎯 Referencias

- **Repositorio GitHub**: https://github.com/kamaro600/net-microservicio-ci-cd
- **SonarCloud**: https://sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd
- **Sitio Web**: https://www.kamaro.online

