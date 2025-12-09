# 🚀 Instrucciones Completas - Frontend Angular

## 📋 Pre-requisitos

1. **Node.js** (versión 18 o superior)
   - Verificar: `node --version`
   - Descargar: https://nodejs.org/

2. **Backend en ejecución**
   - AuthService debe estar en: `http://localhost:5063`
   - WebAPI debe estar en: `http://localhost:5000`

## 🔧 Instalación

### Paso 1: Instalar dependencias
```powershell
cd university-frontend
npm install
```

### Paso 2: Configurar CORS en el Backend

**IMPORTANTE:** Debes agregar CORS en tus servicios .NET para permitir peticiones desde Angular.

#### En `UniversityManagement.WebApi/Program.cs`:
```csharp
// ANTES de builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// DESPUÉS de var app = builder.Build() y ANTES de app.UseAuthorization()
app.UseCors("AllowAngular");
```

#### En `UniversityManagement.AuthService/Program.cs`:
```csharp
// Agregar exactamente la misma configuración CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// En el pipeline
app.UseCors("AllowAngular");
```

### Paso 3: Iniciar el Backend

```powershell
# En la raíz del proyecto (donde está docker-compose.yml)
docker-compose up -d
```

Esperar a que todos los servicios estén levantados (PostgreSQL, Kafka, RabbitMQ, y los microservicios).

### Paso 4: Iniciar Angular

```powershell
cd university-frontend
npm start
```

El servidor de desarrollo se iniciará en `http://localhost:4200`

## 🎯 Uso del Sistema

### 1. Login
- **URL:** `http://localhost:4200/login`
- **Credenciales de ejemplo:**
  - Usuario: `admin@university.com` / Password: `Admin123!`
  - Usuario: `staff@university.com` / Password: `Staff123!`

### 2. Funcionalidades Disponibles

#### 📚 Estudiantes (`/students`)
- Buscar estudiante por ID
- Crear nuevo estudiante
- Editar estudiante existente
- Eliminar estudiante
- **Campos:** FirstName, LastName, Email, DateOfBirth, EnrollmentDate, Address

#### 👨‍🏫 Profesores (`/professors`)
- Listar todos los profesores
- Buscar por nombre
- Crear/Editar/Eliminar profesor
- **Campos:** FirstName, LastName, Email, Specialty, HireDate, IsActive

#### 🏛️ Facultades (`/faculties`)
- Listar todas las facultades
- Buscar por nombre
- Crear/Editar/Eliminar facultad
- **Campos:** Name, Dean, Email, Phone, FoundedDate

#### 🎓 Carreras (`/careers`)
- Listar todas las carreras
- Filtrar por facultad
- Crear/Editar/Eliminar carrera
- **Campos:** Name, Faculty (dropdown), DurationInYears

#### 📝 Matrículas (`/enrollments`)
- **Matricular:** Ingresar ID de estudiante y seleccionar carrera
- **Desmatricular:** Ingresar ID de estudiante y seleccionar carrera
- **Consultar:** Ver todas las matrículas de un estudiante por ID

## 🔍 Verificación de Conectividad

### Probar endpoints del backend:

```powershell
# Auth Service - Login
curl -X POST http://localhost:5063/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"admin@university.com","password":"Admin123!"}'

# WebAPI - Listar estudiantes (necesita token)
curl http://localhost:5000/api/students `
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 🛠️ Scripts Disponibles

```powershell
# Desarrollo (puerto 4200)
npm start

# Build de producción
npm run build

# Ejecutar tests
npm test

# Linting
npm run lint
```

## 📁 Estructura del Proyecto

```
university-frontend/
├── src/
│   ├── app/
│   │   ├── core/               # Servicios, modelos, guards, interceptors
│   │   │   ├── services/       # API services
│   │   │   ├── models/         # TypeScript interfaces
│   │   │   ├── guards/         # Auth guard
│   │   │   └── interceptors/   # HTTP interceptor
│   │   └── features/           # Componentes por funcionalidad
│   │       ├── auth/           # Login
│   │       ├── students/       # CRUD Estudiantes
│   │       ├── professors/     # CRUD Profesores
│   │       ├── faculties/      # CRUD Facultades
│   │       ├── careers/        # CRUD Carreras
│   │       └── enrollments/    # Matrículas
│   ├── styles.css              # Estilos globales
│   └── index.html
└── package.json
```

## 🔧 Troubleshooting

### Error: CORS bloqueado
**Solución:** Verificar que agregaste la configuración CORS en Program.cs de ambos servicios (.NET).

### Error: 401 Unauthorized
**Solución:** 
1. Verificar que el token JWT sea válido
2. Revisar que el interceptor esté agregando el header Authorization
3. Confirmar que el JWT_SECRET_KEY sea el mismo en todos los servicios

### Error: Cannot connect to backend
**Solución:**
1. Verificar que docker-compose esté corriendo: `docker-compose ps`
2. Verificar logs: `docker-compose logs auth-service` o `docker-compose logs web-api`
3. Confirmar que los puertos 5000 y 5063 estén disponibles

### Error: Network error en peticiones
**Solución:** Revisar `environment.ts` y confirmar las URLs:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  authUrl: 'http://localhost:5063/api/auth'
};
```

## 📊 Endpoints del Backend (Referencia)

### Auth Service (puerto 5063)
- `POST /api/auth/login` - Login de usuario

### WebAPI (puerto 5000)
- **Estudiantes:**
  - `GET /api/students/{id}`
  - `POST /api/students`
  - `PUT /api/students/{id}`
  - `DELETE /api/students/{id}`

- **Profesores:**
  - `GET /api/professors?name={search}`
  - `GET /api/professors/{id}`
  - `POST /api/professors`
  - `PUT /api/professors/{id}`
  - `DELETE /api/professors/{id}`

- **Facultades:**
  - `GET /api/faculties?name={search}`
  - `GET /api/faculties/{id}`
  - `POST /api/faculties`
  - `PUT /api/faculties/{id}`
  - `DELETE /api/faculties/{id}`

- **Carreras:**
  - `GET /api/careers`
  - `GET /api/careers/faculty/{facultyId}`
  - `GET /api/careers/{id}`
  - `POST /api/careers`
  - `PUT /api/careers/{id}`
  - `DELETE /api/careers/{id}`

- **Matrículas:**
  - `POST /api/enrollment/enroll`
  - `POST /api/enrollment/unenroll`
  - `GET /api/enrollment/student/{studentId}`

## ✅ Checklist de Verificación

- [ ] Node.js instalado (v18+)
- [ ] Backend corriendo con docker-compose
- [ ] CORS configurado en Program.cs de AuthService
- [ ] CORS configurado en Program.cs de WebAPI
- [ ] `npm install` ejecutado exitosamente
- [ ] `npm start` inicia sin errores
- [ ] Login funciona correctamente
- [ ] Token JWT se almacena en localStorage
- [ ] Navegación entre módulos funciona
- [ ] Peticiones a la API se ejecutan correctamente

## 🎨 Características del Frontend

- ✅ **Standalone Components** (Angular 18)
- ✅ **Autenticación JWT** con interceptor automático
- ✅ **Guards** para proteger rutas
- ✅ **Lazy Loading** de componentes
- ✅ **FormsModule** para formularios template-driven
- ✅ **Diseño responsive** con CSS Grid y Flexbox
- ✅ **Modales** para crear/editar registros
- ✅ **Búsqueda** y filtrado en listas
- ✅ **Manejo de errores** con mensajes al usuario
- ✅ **Estados de carga** (loading spinners)

## 📝 Notas Importantes

1. **No se modificó el backend** - El frontend usa únicamente los endpoints existentes
2. **JWT Secret:** Asegúrate que sea el mismo en todos los servicios (.env y appsettings.json)
3. **Base de datos:** Debe tener datos de prueba para faculties antes de crear careers
4. **Roles:** Solo usuarios Admin/Staff pueden hacer operaciones CRUD
5. **Matrícula:** Requiere que existan tanto el estudiante como la carrera

## 🔄 Flujo Recomendado de Prueba

1. **Login** con credenciales de admin
2. **Crear Facultad** (ej: "Ingeniería")
3. **Crear Carrera** asociada a la facultad
4. **Crear Estudiante**
5. **Crear Profesor**
6. **Matricular estudiante** en la carrera creada
7. **Consultar matrículas** del estudiante
8. **Desmatricular** si es necesario

## 📞 Soporte

Si encuentras algún problema:
1. Revisa los logs del backend: `docker-compose logs -f`
2. Revisa la consola del navegador (F12)
3. Verifica la pestaña Network en DevTools para ver las peticiones HTTP
4. Confirma que todos los servicios de Docker estén en estado "healthy"

---

**Versiones:**
- Angular: 18.2.0
- TypeScript: 5.4.5
- Node.js: 18+ recomendado
- .NET: 9.0
