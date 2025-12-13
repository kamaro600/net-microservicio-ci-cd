# 🔍 Configuración de SonarQube/SonarCloud para Escaneo de Código

## 📋 Problema Actual

El pipeline CI/CD está configurado pero el escaneo de código con SonarQube NO se está ejecutando porque **faltan los secrets necesarios** en GitHub.

## ✅ Solución: Configurar SonarCloud

### Paso 1: Crear Cuenta en SonarCloud (GRATIS)

1. Ve a [SonarCloud.io](https://sonarcloud.io/)
2. Click en **"Log in"** → **"With GitHub"**
3. Autoriza SonarCloud a acceder a tu cuenta de GitHub
4. Selecciona tu organización o cuenta personal

### Paso 2: Crear un Nuevo Proyecto

1. En SonarCloud, click en **"+"** → **"Analyze new project"**
2. Selecciona el repositorio: **`net-microservicio-ci-cd`**
3. Click en **"Set Up"**
4. Elige **"With GitHub Actions"**
5. SonarCloud te mostrará:
   - Tu **Organization Key**
   - Tu **Project Key** (debería ser: `kamaro600_net-microservicio-ci-cd`)
   - Un **Token** (guárdalo, solo se muestra una vez)

### Paso 3: Configurar Secrets en GitHub

Ve a tu repositorio en GitHub:

1. **Settings** → **Secrets and variables** → **Actions**
2. Click en **"New repository secret"**
3. Agrega los siguientes secrets:

#### Secret 1: SONAR_TOKEN
```
Nombre: SONAR_TOKEN
Valor: [El token que te dio SonarCloud]
```

#### Secret 2: SONAR_HOST_URL
```
Nombre: SONAR_HOST_URL
Valor: https://sonarcloud.io
```

### Paso 4: Actualizar el Archivo de Configuración

El archivo `sonar-project.properties` debe tener el **projectKey correcto**:

```properties
sonar.projectKey=kamaro600_net-microservicio-ci-cd
sonar.organization=kamaro600
```

### Paso 5: Actualizar el Pipeline

El archivo `.github/workflows/ci-cd.yml` necesita usar el projectKey correcto en la línea 93:

```yaml
- name: Begin SonarQube analysis
  env:
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
    SONAR_HOST_URL: ${{ secrets.SONAR_HOST_URL }}
  run: |
    dotnet sonarscanner begin \
      /k:"kamaro600_net-microservicio-ci-cd" \
      /o:"kamaro600" \
      /d:sonar.host.url="https://sonarcloud.io" \
      /d:sonar.token="${{ secrets.SONAR_TOKEN }}" \
      /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.cobertura.xml" \
      /d:sonar.coverage.exclusions="**Tests**.cs,**/Program.cs,**/Migrations/**"
```

## 🚀 Ejecución del Pipeline

Una vez configurados los secrets, el pipeline ejecutará automáticamente:

1. ✅ **Build & Test** → Compila y ejecuta tests
2. ✅ **SonarQube Analysis** → Analiza calidad de código
3. ✅ **Build Frontend** → Compila Angular
4. ✅ **Build Docker Images** → Crea imágenes Docker
5. ✅ **Deploy Railway** → Despliega servicios

## 🔍 Verificar el Análisis

Después del push:

1. Ve a **Actions** en tu repositorio GitHub
2. Verifica que el job **"SonarQube Code Quality Analysis"** se ejecute correctamente
3. Ve a [SonarCloud.io](https://sonarcloud.io) para ver el reporte de calidad

## 📊 Métricas que SonarCloud Analiza

- 🐛 **Bugs**: Errores en el código
- 🔒 **Vulnerabilidades**: Problemas de seguridad
- 💡 **Code Smells**: Código que puede mejorarse
- 📈 **Cobertura**: % de código cubierto por tests
- 🔄 **Duplicación**: Código duplicado
- 🎯 **Complejidad**: Complejidad ciclomática

## ❌ Problemas Comunes

### Error: "SONAR_TOKEN not found"
**Solución**: Verifica que agregaste el secret `SONAR_TOKEN` en GitHub Settings.

### Error: "Project not found"
**Solución**: Verifica que el `projectKey` en el pipeline coincida con el de SonarCloud.

### Error: "Unauthorized"
**Solución**: Regenera el token en SonarCloud y actualiza el secret en GitHub.

### El job se salta (skipped)
**Solución**: El job `sonarqube-analysis` depende de `build-and-test-dotnet`. Si ese job falla, SonarQube no se ejecuta.

## 🔄 Orden de Dependencias

```
build-and-test-dotnet
       ↓
sonarqube-analysis ←→ build-frontend
       ↓                    ↓
   build-docker-images ←────┘
       ↓
   deploy-railway
```

## 📝 Alternativa: Usar SonarQube Local (Docker)

Si prefieres no usar SonarCloud, puedes ejecutar SonarQube localmente:

```bash
docker run -d --name sonarqube \
  -p 9000:9000 \
  -e SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true \
  sonarqube:latest
```

Luego configura:
- `SONAR_HOST_URL=http://localhost:9000`
- `SONAR_TOKEN=[token generado en http://localhost:9000]`

## 🎯 Comandos Útiles para Debugging

### Ver logs del pipeline:
Ve a GitHub Actions → Selecciona el run → Click en "SonarQube Code Quality Analysis"

### Ejecutar análisis localmente:
```bash
dotnet tool install --global dotnet-sonarscanner

dotnet sonarscanner begin \
  /k:"kamaro600_net-microservicio-ci-cd" \
  /o:"kamaro600" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.token="TU_TOKEN"

dotnet build

dotnet sonarscanner end /d:sonar.token="TU_TOKEN"
```

## ✅ Checklist de Verificación

- [ ] Cuenta de SonarCloud creada
- [ ] Proyecto en SonarCloud configurado
- [ ] Secret `SONAR_TOKEN` agregado en GitHub
- [ ] Secret `SONAR_HOST_URL` agregado en GitHub
- [ ] `sonar-project.properties` actualizado con projectKey correcto
- [ ] Pipeline actualizado con organization y projectKey correctos
- [ ] Push realizado para activar el pipeline
- [ ] Job "SonarQube Analysis" se ejecuta exitosamente
- [ ] Reporte visible en SonarCloud dashboard

## 📚 Recursos Adicionales

- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [SonarScanner for .NET](https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/)
- [GitHub Actions with SonarCloud](https://github.com/SonarSource/sonarcloud-github-action)
