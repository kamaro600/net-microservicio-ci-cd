# Architectural Decision Records (ADR)
# Sistema de Gestión Universitaria

Este documento contiene las decisiones arquitectónicas clave tomadas durante el desarrollo del sistema.

---

## ADR-001: Adopción de Arquitectura de Microservicios con .NET 9.0

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se necesita construir un sistema de gestión universitaria con capacidad de escalar componentes independientemente y permitir equipos autónomos. El equipo de desarrollo tiene experiencia sólida con el ecosistema .NET y C#.

**Decisión:**
Adoptamos una arquitectura de microservicios utilizando .NET 9.0 como framework principal para el backend, con los siguientes servicios independientes:
- **AuthService**: Autenticación y autorización
- **WebAPI**: Lógica de negocio principal (estudiantes, cursos, inscripciones)
- **NotificationService**: Envío de notificaciones
- **AuditService**: Auditoría y trazabilidad de eventos

**Alternativas Consideradas:**
1. **Monolito .NET**: Más simple inicialmente, pero dificulta el escalado independiente
2. **Node.js/Express**: No hay experiencia del equipo, curva de aprendizaje alta
3. **Spring Boot (Java)**: Excelente para microservicios, pero el equipo no tiene experiencia
4. **Python (FastAPI)**: Buena opción, pero el equipo prefiere tipado fuerte de C#

**Razones de la Decisión:**
- ✅ **Conocimiento del equipo**: Dominio sólido de C# y .NET, reduciendo curva de aprendizaje
- ✅ **Ecosistema maduro**: .NET 9.0 ofrece alto rendimiento, herramientas robustas (EF Core, SignalR)
- ✅ **Tipado fuerte**: Reduce errores en tiempo de ejecución
- ✅ **Escalabilidad**: Cada microservicio puede escalar independientemente
- ✅ **Despliegue independiente**: Permite releases sin afectar todo el sistema
- ✅ **Resiliencia**: Fallos aislados por servicio

**Consecuencias:**

**Positivas:**
- Velocidad de desarrollo alta gracias a conocimiento previo
- Mantenibilidad mejorada con separación de responsabilidades
- Escalado granular según demanda por servicio
- Tecnología moderna con soporte LTS hasta 2027

**Negativas:**
- Mayor complejidad operacional vs monolito
- Necesidad de orquestación (Docker, Kubernetes/Railway)
- Comunicación entre servicios requiere gestión de latencia
- Monitoreo distribuido más complejo

**Mitigación de Riesgos:**
- Uso de Docker para consistencia en ambientes
- Pipeline CI/CD automatizado para deployment
- Event-driven architecture con RabbitMQ/Kafka para desacoplamiento
- Observabilidad con logs centralizados

---

## ADR-002: PostgreSQL como Base de Datos Principal

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se requiere una base de datos relacional robusta para almacenar datos transaccionales del sistema universitario (estudiantes, cursos, inscripciones, auditoría). El equipo tiene experiencia previa con PostgreSQL.

**Decisión:**
Utilizamos **PostgreSQL 16** como base de datos principal para todos los microservicios, con instancias separadas por servicio siguiendo el patrón "Database per Service".

**Alternativas Consideradas:**
1. **MySQL**: Popular, pero PostgreSQL ofrece características avanzadas (JSONB, window functions)
2. **MongoDB**: NoSQL, no adecuado para relaciones complejas del dominio universitario
3. **SQL Server**: Excelente, pero costos de licenciamiento en producción
4. **Oracle**: Sobrecapacidad para el proyecto, costos prohibitivos

**Razones de la Decisión:**
- ✅ **Conocimiento del equipo**: Experiencia sólida con PostgreSQL y SQL estándar
- ✅ **Open Source**: Sin costos de licencia, ideal para presupuestos educativos
- ✅ **Características avanzadas**: JSONB, índices GIN/GiST, ventanas, CTEs
- ✅ **Integridad referencial**: FK, constraints, triggers para reglas de negocio
- ✅ **Performance**: Excelente rendimiento para OLTP y consultas analíticas
- ✅ **Comunidad activa**: Gran soporte y extensiones (PostGIS, pg_trgm)
- ✅ **Cloud-ready**: Soporte nativo en Railway, AWS RDS, Azure, GCP

**Consecuencias:**

**Positivas:**
- Sin costos de licencia en ningún ambiente
- Excelente para modelado de relaciones universitarias
- Soporte nativo de JSON para configuraciones flexibles
- Replicación y backup bien soportados
- Compatible con ORM Entity Framework Core

**Negativas:**
- Requiere gestión de múltiples instancias (una por microservicio)
- Necesidad de monitoreo de conexiones pool
- Transacciones distribuidas son complejas (evitadas con eventos)

**Implementación:**
- **Auth Service**: PostgreSQL con esquema `auth_db`
- **WebAPI**: PostgreSQL con esquema `university_db`
- **Audit Service**: PostgreSQL con esquema `audit_db`
- Cada servicio con su propia cadena de conexión
- Hosted en **Neon** (PostgreSQL serverless) para ambiente de producción

---

## ADR-003: Angular 18 para el Frontend

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se necesita un framework frontend moderno para construir una Single Page Application (SPA) que consuma las APIs de los microservicios. El equipo tiene experiencia con TypeScript y frameworks modernos.

**Decisión:**
Utilizamos **Angular 18** con TypeScript como framework para el frontend web del sistema universitario.

**Alternativas Consideradas:**
1. **React**: Popular, pero requiere más decisiones de arquitectura (routing, state management)
2. **Vue.js**: Más simple, pero menos estructura empresarial
3. **Svelte**: Moderno y performante, pero menos maduro y menos comunidad
4. **Blazor**: Nativo de .NET, pero el equipo prefiere separación frontend/backend

**Razones de la Decisión:**
- ✅ **Framework completo**: Incluye routing, HTTP client, forms, DI out-of-the-box
- ✅ **TypeScript nativo**: Tipado fuerte, mejor autocompletado, menos errores
- ✅ **Arquitectura estructurada**: Enforces best practices con módulos y servicios
- ✅ **Ecosystem maduro**: Angular Material, RxJS, CLI poderoso
- ✅ **Performance**: Standalone components, señales (signals), SSR opcional
- ✅ **Enterprise-ready**: Usado en producción por Google, Microsoft, IBM
- ✅ **Conocimiento del equipo**: Experiencia previa con Angular

**Consecuencias:**

**Positivas:**
- Desarrollo rápido con Angular CLI (scaffolding automático)
- Separación clara de responsabilidades (components, services, guards)
- Testing integrado (Jasmine/Karma)
- Reactive programming con RxJS para manejo de streams
- Lazy loading de módulos para optimización

**Negativas:**
- Bundle size más grande que frameworks alternativos
- Curva de aprendizaje moderada para nuevos miembros
- Actualizaciones frecuentes requieren mantenimiento

**Implementación:**
- Estructura modular: `auth`, `dashboard`, `students`, `courses`
- Comunicación con backend vía `HttpClient` con interceptors para JWT
- State management simple con servicios (sin Redux/NgRx por simplicidad)
- Deployment como contenedor Docker con Nginx

---

## ADR-004: GitHub y GitHub Actions para CI/CD

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se requiere control de versiones, colaboración en código y automatización de pipelines CI/CD. El presupuesto es limitado (proyecto educativo) y se busca integración estrecha entre código y deployment.

**Decisión:**
Utilizamos **GitHub** como plataforma de versionamiento y **GitHub Actions** para pipelines CI/CD completos.

**Alternativas Consideradas:**
1. **GitLab**: CI/CD integrado, pero menos familiar para el equipo
2. **Bitbucket + Jenkins**: Más configuración manual, overhead operacional
3. **Azure DevOps**: Excelente para .NET, pero menos comunidad y recursos públicos
4. **CircleCI**: Potente, pero con límites en plan gratuito

**Razones de la Decisión:**
- ✅ **Gratuidad**: Repositorios públicos ilimitados, 2000 minutos/mes de Actions
- ✅ **Integración nativa**: Actions integrado directamente en el repositorio
- ✅ **Ecosistema extenso**: Marketplace con miles de actions pre-construidas
- ✅ **GitHub Packages**: Container registry (ghcr.io) incluido gratuitamente
- ✅ **Colaboración**: Pull Requests, Code Review, Issues integrados
- ✅ **Documentación**: README.md renderizado, GitHub Pages para docs
- ✅ **Comunidad**: Plataforma más usada, fácil compartir y reclutar

**Consecuencias:**

**Positivas:**
- Sin costos de infraestructura CI/CD
- Pipelines como código (`.github/workflows/`)
- Matriz de builds para múltiples servicios en paralelo
- Secrets management integrado
- GitHub Container Registry público para imágenes Docker
- Integración con SonarCloud para Quality Gates

**Negativas:**
- Vendor lock-in parcial (aunque Actions usa sintaxis similar a otros CI/CD)
- Límite de 2000 minutos/mes en plan gratuito (suficiente para el proyecto)
- Runners compartidos pueden ser más lentos que dedicados

**Implementación:**
- Pipeline multi-stage: Build → Test → SonarQube → Docker Build → Deploy
- Publicación a `ghcr.io/kamaro600/net-microservicio-ci-cd`
- Secrets: `SONAR_TOKEN`, `SONAR_HOST_URL`, `GITHUB_TOKEN`
- Deployment automático a Railway mediante "Wait for CI"

---

## ADR-005: Railway como Plataforma de Hosting

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se necesita una plataforma de hosting para desplegar los microservicios en producción. El presupuesto es limitado (proyecto educativo) y se busca simplicidad operacional sin sacrificar capacidades modernas.

**Decisión:**
Utilizamos **Railway** como plataforma de hosting para todos los servicios del sistema.

**Alternativas Consideradas:**
1. **AWS (ECS/EKS)**: Potente, pero complejo y costoso para proyecto educativo
2. **Google Cloud Run**: Serverless excelente, pero requiere configuración compleja
3. **Azure Container Apps**: Buena integración con .NET, pero curva de aprendizaje
4. **Heroku**: Pionero, pero deprecó plan gratuito en 2022
5. **DigitalOcean App Platform**: Buen precio, pero menos features que Railway
6. **Render**: Similar a Railway, pero menos generoso en plan gratuito

**Razones de la Decisión:**
- ✅ **Capa gratuita generosa**: $5 USD/mes de créditos, suficiente para 5 servicios pequeños
- ✅ **Simplicidad extrema**: Deploy automático desde GitHub sin configuración compleja
- ✅ **PostgreSQL incluido**: Base de datos managed sin costo adicional (Neon)
- ✅ **Dockerfile nativo**: Soporta builds desde Dockerfiles directamente
- ✅ **Variables de entorno**: Gestión sencilla de configuración por ambiente
- ✅ **Dominios personalizados**: SSL automático con Let's Encrypt
- ✅ **"Wait for CI"**: Integración con GitHub Actions para desplegar solo si CI pasa
- ✅ **Logs en tiempo real**: Debugging sencillo sin configuración adicional
- ✅ **Escalado sencillo**: Cambio de plan cuando se requiera

**Consecuencias:**

**Positivas:**
- Deploy automatizado sin DevOps complejo
- Sin gestión de infraestructura (serverless/PaaS)
- Ideal para MVP y proyectos educativos
- Monitoreo básico incluido
- Rápido time-to-market

**Negativas:**
- Vendor lock-in moderado (aunque usa contenedores estándar)
- Menos control que IaaS (AWS EC2, VMs)
- Plan gratuito con limitaciones de CPU/RAM
- No apto para alta escala productiva (pero suficiente para demo/educación)

**Implementación:**
- 5 servicios desplegados: Auth, WebAPI, Notification, Audit, Frontend
- Cada servicio con su Dockerfile en el repositorio
- Variables de entorno configuradas en Railway dashboard
- Auto-deploy habilitado con "Wait for CI"
- Dominio personalizado: `www.kamaro.online` apuntando al frontend

**Plan de Escalabilidad:**
Si el proyecto crece, migración a:
- **Azure Container Apps** (para mantener stack .NET)
- **AWS ECS Fargate** (para más control)
- **Kubernetes** auto-gestionado en DigitalOcean/Linode

---

## ADR-006: Event-Driven Architecture con RabbitMQ y Kafka

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Los microservicios necesitan comunicarse de forma asincrónica para desacoplamiento y resiliencia. Se requiere notificaciones, auditoría y sincronización de datos entre servicios.

**Decisión:**
Implementamos comunicación event-driven usando:
- **RabbitMQ**: Para notificaciones y mensajería transaccional
- **Kafka**: Para streaming de eventos de auditoría

**Alternativas Consideradas:**
1. **Solo REST**: Simple pero acoplamiento fuerte y sincrónico
2. **Azure Service Bus**: Managed, pero costos y vendor lock-in
3. **AWS SQS/SNS**: Managed, pero fragmentado (múltiples servicios)
4. **Redis Pub/Sub**: Ligero, pero sin persistencia garantizada

**Razones de la Decisión:**
- ✅ **Desacoplamiento**: Servicios no necesitan conocerse entre sí
- ✅ **Resiliencia**: Mensajes persisten si un servicio cae
- ✅ **Escalabilidad**: Procesamiento asincrónico de alta carga
- ✅ **Patrón Pub/Sub**: Múltiples consumidores para un evento
- ✅ **RabbitMQ**: Ideal para mensajería transaccional garantizada
- ✅ **Kafka**: Ideal para logs de auditoría y event sourcing

**Consecuencias:**

**Positivas:**
- Servicios independientes sin acoplamiento temporal
- Retry automático de mensajes fallidos
- Auditoría completa de eventos del sistema

**Negativas:**
- Complejidad adicional de gestionar message brokers
- Eventual consistency (no inmediatez en algunos casos)
- Necesidad de monitoreo de colas

**Implementación:**
- RabbitMQ para: `StudentEnrolled`, `PaymentProcessed`
- Kafka para: `AuditLog`, `SystemEvent`

---

## ADR-007: SonarCloud para Quality Gate en CI/CD

**Estado:** ✅ Aceptado

**Fecha:** Diciembre 2025

**Contexto:**
Se necesita asegurar calidad de código antes de desplegar a producción. El equipo busca análisis estático automatizado integrado en el pipeline.

**Decisión:**
Implementamos **SonarCloud** como Quality Gate obligatorio en el pipeline CI/CD.

**Alternativas Consideradas:**
1. **SonarQube self-hosted**: Mayor control, pero requiere infraestructura
2. **CodeClimate**: Bueno, pero menos enfocado en .NET
3. **DeepSource**: Moderno, pero menos maduro
4. **Solo análisis local**: Sin enforcement automatizado

**Razones de la Decisión:**
- ✅ **Gratuito para open source**: Sin costo para repositorios públicos
- ✅ **Soporte .NET nativo**: Scanner oficial para C#
- ✅ **Integración GitHub**: Pull Request decoration automática
- ✅ **Métricas completas**: Bugs, vulnerabilities, code smells, coverage
- ✅ **Quality Gate customizable**: Define umbrales que bloquean deployment

**Consecuencias:**

**Positivas:**
- Calidad de código enforced automáticamente
- Detección temprana de bugs y vulnerabilidades
- Métricas visibles en dashboard público
- Bloqueo automático si calidad baja

**Negativas:**
- Añade 3-5 minutos al pipeline
- Requiere configuración de umbrales adecuados

**Implementación:**
- Quality Gate: Coverage > 75%, Bugs = 0, Vulnerabilities = 0
- Pipeline falla si no se cumple
- Dashboard: https://sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd

---

## Resumen de Decisiones

| ADR | Decisión | Razón Principal | Estado |
|-----|----------|-----------------|--------|
| 001 | Microservicios .NET 9.0 | Conocimiento del equipo + Escalabilidad | ✅ Aceptado |
| 002 | PostgreSQL | Conocimiento + Open Source | ✅ Aceptado |
| 003 | Angular 18 | Framework completo + TypeScript | ✅ Aceptado |
| 004 | GitHub Actions | Gratuidad + Integración | ✅ Aceptado |
| 005 | Railway | Capa gratuita + Simplicidad | ✅ Aceptado |
| 006 | RabbitMQ + Kafka | Desacoplamiento + Event-driven | ✅ Aceptado |
| 007 | SonarCloud | Quality Gate automatizado | ✅ Aceptado |

---

## Proceso de Actualización de ADRs

**Cuándo crear un ADR:**
- Cambio significativo de tecnología
- Decisión que impacta múltiples componentes
- Trade-off importante entre alternativas

**Formato de ADR:**
- Estado (Propuesto, Aceptado, Rechazado, Deprecado)
- Contexto y problema
- Decisión tomada
- Alternativas consideradas
- Consecuencias (positivas y negativas)

**Versionamiento:**
Los ADRs se versionan en Git junto con el código, garantizando trazabilidad histórica.

---

**Última actualización:** Diciembre 2025
