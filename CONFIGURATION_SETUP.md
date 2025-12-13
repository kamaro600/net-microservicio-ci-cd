# Configuración de Archivos Sensibles

Este documento describe cómo configurar los archivos de configuración sensibles necesarios para ejecutar la aplicación.

## ⚠️ Archivos que Debes Configurar

### 1. Archivos appsettings.json

Los archivos `appsettings.json` y `appsettings.Production.json` en cada servicio contienen valores de ejemplo que **DEBES** reemplazar con tus credenciales reales.

#### Servicios a Configurar:

- **UniversityManagement.WebApi/**
  - `appsettings.json`
  - `appsettings.Production.json`

- **UniversityManagement.AuthService/**
  - `appsettings.json`
  - `appsettings.Production.json`

- **UniversityManagement.AuditService/**
  - `appsettings.json`
  - `appsettings.Production.json`

- **UniversityManagement.NotificationService/**
  - `appsettings.json`
  - `appsettings.Production.json`

### 2. Archivo .env (Opcional para Docker)

Si vas a usar Docker Compose, copia el archivo de ejemplo:

```bash
cp .env.example .env
```

Luego edita `.env` con tus valores reales.

## 🔐 Valores que Debes Cambiar

### ConnectionStrings (Cadenas de Conexión)

Reemplaza los valores de ejemplo con tus credenciales de base de datos:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=TU_HOST;Port=5432;Database=TU_DATABASE;Username=TU_USUARIO;Password=TU_PASSWORD"
}
```

### JWT Secret Key

**IMPORTANTE**: Genera una clave secreta segura (mínimo 32 caracteres) para JWT:

```json
"JwtSettings": {
  "SecretKey": "TU_CLAVE_SECRETA_AQUI_MINIMO_32_CARACTERES"
}
```

Para generar una clave segura, puedes usar:

**PowerShell:**
```powershell
-join ((48..57) + (65..90) + (97..122) + (33, 35, 36, 37, 38, 42, 43, 45, 61, 63, 64, 94, 126) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

**Linux/Mac:**
```bash
openssl rand -base64 48
```

### SMTP Settings (Servicio de Notificaciones)

En `UniversityManagement.NotificationService/appsettings.json`:

```json
"SmtpSettings": {
  "Host": "tu-servidor-smtp.com",
  "Port": 587,
  "Username": "tu-usuario-smtp",
  "Password": "tu-password-smtp",
  "EnableSsl": true,
  "FromEmail": "noreply@tudominio.com",
  "FromName": "Universidad - Sistema de Matrículas"
}
```

Opciones de proveedores SMTP:
- **Gmail**: `smtp.gmail.com` (requiere "App Password")
- **SendGrid**: `smtp.sendgrid.net`
- **Mailtrap** (testing): `sandbox.smtp.mailtrap.io`
- **AWS SES**: depende de tu región

## 📝 Archivos de Ejemplo Disponibles

Todos los servicios tienen archivos `.example.json` que puedes usar como referencia:

- `appsettings.example.json` - Plantilla con todos los campos necesarios

**NO** modifiques los archivos `.example.json`, estos son referencias para otros desarrolladores.

## 🚫 Seguridad

**NUNCA** commitees archivos con credenciales reales:
- ✅ Los archivos `appsettings.json` actuales tienen valores de ejemplo
- ✅ El archivo `.env` está en `.gitignore`
- ❌ NO remuevas las entradas del `.gitignore`
- ❌ NO commitees archivos con contraseñas o claves reales

## 🔍 Verificación

Después de configurar, verifica que:

1. ✅ Todos los `appsettings.json` tienen tus credenciales reales
2. ✅ El JWT SecretKey es único y seguro (mínimo 32 caracteres)
3. ✅ Las cadenas de conexión apuntan a tus bases de datos
4. ✅ Las configuraciones SMTP son correctas
5. ✅ NO hay credenciales reales en archivos `.example`

## 🚀 Entornos

- **Desarrollo Local**: Usa `appsettings.json`
- **Producción/Docker**: Usa `appsettings.Production.json`
- **Docker Compose**: Usa archivo `.env`

Las configuraciones de producción tienen prioridad sobre las de desarrollo cuando ambas existen.

## 📚 Recursos Adicionales

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Connection Strings PostgreSQL](https://www.connectionstrings.com/postgresql/)
