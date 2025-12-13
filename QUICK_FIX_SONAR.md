# 🔧 PASOS INMEDIATOS - Activar Escaneo de Código

## ⚠️ PROBLEMA IDENTIFICADO

El pipeline está configurado correctamente, pero **faltan los secrets de SonarCloud** en GitHub. Por eso el job de análisis de código se está saltando o fallando.

## ✅ SOLUCIÓN EN 5 PASOS (15 minutos)

### 📝 PASO 1: Crear Cuenta en SonarCloud
1. Abre: https://sonarcloud.io/
2. Click en **"Log in"** (arriba derecha)
3. Selecciona **"With GitHub"**
4. Autoriza SonarCloud

### 📝 PASO 2: Importar tu Proyecto
1. Una vez dentro de SonarCloud, click en **"+"** (arriba derecha) → **"Analyze new project"**
2. Busca y selecciona: **`net-microservicio-ci-cd`**
3. Click en **"Set Up"**
4. Si te pregunta el método de análisis:
   - Selecciona **"With GitHub Actions"** o **"GitHub Actions"**
   - Si NO aparece esa opción, selecciona **"Other CI"** o **"Manually"**

### 📝 PASO 3: Generar Token de SonarCloud

#### Opción A: Si SonarCloud te muestra el token automáticamente
SonarCloud puede mostrarte una pantalla con:
```
Organization: kamaro600
Project Key: kamaro600_net-microservicio-ci-cd
Token: [un código largo] ← ⚠️ COPIA ESTO AHORA
```

#### Opción B: Generar Token Manualmente (si no se mostró)
1. En SonarCloud, click en tu avatar (arriba derecha)
2. **"My Account"** → **"Security"** tab
3. En **"Generate Tokens"**:
   - Token Name: `GitHub Actions`
   - Type: **"Global Analysis Token"** o **"User Token"**
   - Expires in: **"No expiration"** o **"90 days"**
4. Click **"Generate"**
5. **⚠️ COPIA EL TOKEN** (solo se muestra una vez)

### 📝 PASO 3.5: Verificar Organization y Project Key

Ve a tu proyecto en SonarCloud y verifica:
- URL será algo como: `https://sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd`
- El **Project Key** está después de `?id=`
- Tu **Organization** es: `kamaro600`

### 📝 PASO 4: Agregar Secrets en GitHub

1. Ve a tu repositorio: https://github.com/kamaro600/net-microservicio-ci-cd
2. Click en **"Settings"** (tab superior)
3. En el menú izquierdo: **"Secrets and variables"** → **"Actions"**
4. Click en **"New repository secret"**

Agrega estos 2 secrets:

#### Secret 1:
```
Name: SONAR_TOKEN
Secret: [pega el token que copiaste de SonarCloud]
```
Click en **"Add secret"**

#### Secret 2:
```
Name: SONAR_HOST_URL
Secret: https://sonarcloud.io
```
Click en **"Add secret"**

### 📝 PASO 5: Hacer Commit y Push

Abre PowerShell en tu proyecto y ejecuta:

```powershell
git add .
git commit -m "feat: Configure SonarCloud integration for code scanning"
git push origin main
```

## ✅ VERIFICACIÓN

1. Ve a: https://github.com/kamaro600/net-microservicio-ci-cd/actions
2. Verás un nuevo workflow ejecutándose
3. Click en el workflow → Click en el job **"SonarQube Code Quality Analysis"**
4. Deberías ver logs como:
   ```
   ✓ Installing SonarScanner
   ✓ Beginning analysis
   ✓ Building project
   ✓ Ending analysis
   ✓ Quality Gate: PASSED
   ```

5. Ve a: https://sonarcloud.io/dashboard?id=kamaro600_net-microservicio-ci-cd
   - Verás el dashboard con métricas de calidad de código

## 🎯 QUÉ ESPERAR

Una vez configurado, cada push activará:

```
✅ Build & Test .NET → Compila y ejecuta tests
✅ SonarQube Analysis → Escanea código (ESTO ES LO QUE FALTA)
✅ Build Frontend → Compila Angular
✅ Build Docker Images → Crea imágenes
✅ Deploy Railway → Despliega servicios
```

## 📊 Métricas que Verás en SonarCloud

- **Bugs**: 🐛 Errores detectados
- **Vulnerabilities**: 🔒 Problemas de seguridad
- **Code Smells**: 💡 Código mejorable
- **Coverage**: 📈 Cobertura de tests
- **Duplications**: 🔄 Código duplicado
- **Security Hotspots**: ⚠️ Posibles riesgos

## ❌ SI ALGO FALLA

### Error: "SONAR_TOKEN not found"
→ Revisa que agregaste el secret en: Settings → Secrets → Actions

### Error: "Project key not found"
→ Ya está corregido en el último commit

### El job se "salta" (skipped)
→ Verifica que el job anterior "Build & Test" se ejecutó exitosamente

## 🔍 DIAGNÓSTICO RÁPIDO

Si después de hacer push quieres verificar:

```powershell
# Ver estado del último run
https://github.com/kamaro600/net-microservicio-ci-cd/actions

# Ver configuración de secrets
https://github.com/kamaro600/net-microservicio-ci-cd/settings/secrets/actions
```

## 📞 AYUDA ADICIONAL

Si necesitas regenerar el token:
1. Ve a: https://sonarcloud.io/account/security
2. Generate new token
3. Actualiza el secret en GitHub

## ✨ RESULTADO FINAL

Una vez completados estos pasos, tu pipeline tendrá:
- ✅ Análisis automático de calidad de código
- ✅ Detección de bugs y vulnerabilidades
- ✅ Métricas de cobertura de tests
- ✅ Quality Gate que previene código de mala calidad
- ✅ Dashboard visual en SonarCloud
