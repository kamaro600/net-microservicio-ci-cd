# 🎓 Frontend Angular - Sistema Universidad

Frontend desarrollado en **Angular 18** (standalone components) para el sistema de gestión universitaria.

## 📋 Características

✅ **CRUD Completo:**
- Estudiantes
- Profesores  
- Facultades
- Carreras

✅ **Operaciones de Matrícula:**
- Matricular estudiante en carrera
- Desmatricular estudiante
- Ver matrículas de un estudiante

✅ **Autenticación JWT**
✅ **Diseño Responsivo**
✅ **Standalone Components (Angular 18)**

## 🚀 Instalación Rápida

### Prerrequisitos
- **Node.js 18+** y **npm**
- Backend .NET corriendo en `localhost:5000` y `localhost:5063`

### Paso 1: Instalar dependencias

```bash
cd university-frontend
npm install
```

### Paso 2: Iniciar servidor de desarrollo

```bash
npm start
```

La aplicación estará disponible en **`http://localhost:4200`**

## 📁 Estructura del Proyecto

```
university-frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── models/          # Interfaces TypeScript
│   │   │   ├── services/        # Servicios HTTP
│   │   │   ├── interceptors/    # HTTP Interceptors
│   │   │   └── config/          # Configuración (environment)
│   │   ├── features/
│   │   │   ├── auth/            # Login
│   │   │   ├── students/        # CRUD Estudiantes
│   │   │   ├── professors/      # CRUD Profesores
│   │   │   ├── faculties/       # CRUD Facultades
│   │   │   ├── careers/         # CRUD Carreras
│   │   │   └── enrollments/     # Matrículas
│   │   ├── app.component.ts     # Componente raíz
│   │   └── app.routes.ts        # Configuración de rutas
│   ├── styles.css               # Estilos globales
│   └── index.html
├── package.json
├── angular.json
└── tsconfig.json
```

## 🔑 Credenciales de Prueba

```
Admin:
  Email: admin@universidad.edu
  Password: Admin123

Staff:
  Email: staff@universidad.edu  
  Password: Staff123
```

## 🌐 Endpoints de la API

El frontend se conecta a:
- **API Principal:** `http://localhost:5000/api`
- **Auth Service:** `http://localhost:5063/api`

Configuración en: `src/app/core/config/environment.ts`

## 📝 Componentes Principales

### 1. Login (`/login`)
- Autenticación con JWT
- Almacena token en localStorage
- Redirección automática

### 2. Estudiantes (`/students`)
- Buscar por ID
- Crear, editar, eliminar
- Formulario modal

### 3. Profesores (`/professors`)
- Lista completa
- Búsqueda por término
- CRUD completo

### 4. Facultades (`/faculties`)
- Lista completa
- Gestión de decanos
- CRUD completo

### 5. Carreras (`/careers`)
- Filtro por facultad
- Duración en años
- CRUD completo

### 6. Matrículas (`/enrollments`)
- Matricular estudiante
- Desmatricular
- Ver matrículas activas

## 🎨 Estilos

El proyecto usa **CSS puro** con variables y gradientes modernos:
- Tema principal: Gradiente púrpura/azul
- Cards con sombras suaves
- Botones con hover effects
- Diseño responsive
- Modal overlays

## 🔧 Configuración Avanzada

### Cambiar URL de la API

Editar `src/app/core/config/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://tu-servidor:5000/api',
  authUrl: 'http://tu-servidor:5063/api'
};
```

### Habilitar CORS en .NET

En `Program.cs` del backend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("AllowAngular");
```

## 📦 Compilación para Producción

```bash
npm run build
```

Los archivos optimizados estarán en `dist/university-frontend/`

### Desplegar con Docker

Crear `Dockerfile` en la raíz del frontend:

```dockerfile
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/university-frontend /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

Construir y ejecutar:

```bash
docker build -t university-frontend .
docker run -p 4200:80 university-frontend
```

## 🐛 Solución de Problemas

### Error de CORS

**Problema:** Peticiones bloqueadas por CORS

**Solución:** Verificar configuración de CORS en el backend .NET

### Token expirado

**Problema:** Error 401 Unauthorized

**Solución:** Volver a hacer login para obtener nuevo token

### Puerto 4200 ocupado

**Problema:** `Port 4200 is already in use`

**Solución:**
```bash
# Cambiar puerto
ng serve --port 4300

# O matar proceso en Windows
netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

## 📚 Tecnologías Utilizadas

- **Angular 18** (Standalone Components)
- **TypeScript 5.4**
- **RxJS 7.8**
- **Angular Router**
- **Angular Forms**
- **HttpClient** con Interceptors

## 🚢 Despliegue Completo con Docker Compose

Agregar al `docker-compose.yml` principal:

```yaml
frontend:
  build:
    context: ./university-frontend
    dockerfile: Dockerfile
  container_name: university_frontend
  ports:
    - "4200:80"
  networks:
    - university-network
  restart: unless-stopped
```

## 🎯 Próximas Mejoras

- [ ] Paginación en listas
- [ ] Validación de formularios más robusta
- [ ] Manejo de errores mejorado
- [ ] Tests unitarios con Jasmine
- [ ] Dashboard con estadísticas
- [ ] Exportación a PDF/Excel

## 📞 Soporte

Para problemas o preguntas sobre el frontend, contactar al equipo de desarrollo.

---

**Desarrollado para el proyecto de Arquitectura de Microservicios**
