# Diagramas de Arquitectura
# Sistema de Gestión Universitaria

**Fecha:** Diciembre 2025

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

## 🔐 Seguridad en la Arquitectura

```mermaid
graph TB
    subgraph "Seguridad de Red"
        HTTPS[🔒 HTTPS/TLS<br/>Let's Encrypt]
        CORS[🛡️ CORS Policy<br/>Dominios Permitidos]
    end

    subgraph "Seguridad de Aplicación"
        JWT[🔑 JWT Tokens<br/>OAuth2 + Bearer]
        Auth[🔐 Authorization<br/>RBAC]
        Validation[✅ Input Validation<br/>Data Sanitization]
    end

    subgraph "Seguridad de Infraestructura"
        Secrets[🔒 Secrets Management<br/>Railway Environment Variables]
        Network[🌐 Network Isolation<br/>Private Networking]
        Container[🐳 Container Security<br/>Non-root Users]
    end

    subgraph "Seguridad de Datos"
        Encryption[🔐 Data Encryption<br/>At Rest + In Transit]
        Backup[💾 Automated Backups<br/>Neon PostgreSQL]
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

## 📈 Escalabilidad y Alta Disponibilidad

```mermaid
graph TB
    subgraph "Escalado Horizontal (Future)"
        LB[⚖️ Load Balancer]
        R1[🐳 Replica 1]
        R2[🐳 Replica 2]
        R3[🐳 Replica 3]
    end

    subgraph "Escalado Vertical (Current)"
        Plan[📦 Railway Plan<br/>CPU: 0.5 vCPU<br/>RAM: 512 MB<br/>→ Upgradeable]
    end

    subgraph "Database Scaling"
        NeonDB[(☁️ Neon PostgreSQL<br/>Auto-scaling<br/>Read Replicas)]
    end

    subgraph "Caching (Future)"
        Redis[⚡ Redis<br/>Session Store<br/>Response Cache]
    end

    LB --> R1
    LB --> R2
    LB --> R3

    R1 --> NeonDB
    R2 --> NeonDB
    R3 --> NeonDB

    R1 --> Redis
    R2 --> Redis
    R3 --> Redis

    style LB fill:#0078d4,color:#fff
    style NeonDB fill:#00cc88,color:#fff
    style Redis fill:#dc382d,color:#fff
    style Plan fill:#000000,color:#fff
```

**Estrategia de Escalabilidad:**

- **Fase 1 (Actual)**: Railway plan gratuito, 1 réplica por servicio
- **Fase 2 (Crecimiento)**: Escalado vertical (más CPU/RAM)
- **Fase 3 (Producción)**: Escalado horizontal (múltiples réplicas)
- **Fase 4 (Enterprise)**: Redis cache + CDN + Multi-región

---

## 🎯 Referencias

- **Repositorio GitHub**: https://github.com/kamaro600/net-microservicio-ci-cd
- **SonarCloud**: https://sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd
- **Sitio Web**: https://www.kamaro.online
- **Documentación ADR**: [ADR.md](ADR.md)
- **Documentación CI/CD**: [CI-CD-README.md](../CI-CD-README.md)

---

**Última actualización:** Diciembre 2025
**Autor:** Equipo de Desarrollo
**Versión:** 1.0
