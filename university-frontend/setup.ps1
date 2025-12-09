# Script de instalación rápida para el frontend Angular
# Ejecutar con: .\setup.ps1

Write-Host "================================" -ForegroundColor Cyan
Write-Host "  Universidad - Frontend Setup  " -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Node.js
Write-Host "Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "✓ Node.js instalado: $nodeVersion" -ForegroundColor Green
    
    $major = [int]($nodeVersion -replace 'v(\d+)\..*', '$1')
    if ($major -lt 18) {
        Write-Host "⚠ Advertencia: Se recomienda Node.js v18 o superior" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Node.js no encontrado" -ForegroundColor Red
    Write-Host "Por favor instala Node.js desde: https://nodejs.org/" -ForegroundColor Red
    exit 1
}

# Verificar npm
Write-Host "Verificando npm..." -ForegroundColor Yellow
try {
    $npmVersion = npm --version
    Write-Host "✓ npm instalado: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ npm no encontrado" -ForegroundColor Red
    exit 1
}

# Instalar dependencias
Write-Host ""
Write-Host "Instalando dependencias de Angular..." -ForegroundColor Yellow
Write-Host "Esto puede tomar unos minutos..." -ForegroundColor Gray

$currentLocation = Get-Location
Set-Location $PSScriptRoot

try {
    npm install
    Write-Host "✓ Dependencias instaladas exitosamente" -ForegroundColor Green
} catch {
    Write-Host "✗ Error al instalar dependencias" -ForegroundColor Red
    Set-Location $currentLocation
    exit 1
}

Set-Location $currentLocation

# Verificar backend
Write-Host ""
Write-Host "Verificando conectividad del backend..." -ForegroundColor Yellow

function Test-Endpoint {
    param($url, $name)
    try {
        $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 5 -ErrorAction Stop
        Write-Host "✓ $name está disponible" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "✗ $name NO está disponible ($url)" -ForegroundColor Red
        return $false
    }
}

$authOk = Test-Endpoint "http://localhost:5063/health" "AuthService"
$apiOk = Test-Endpoint "http://localhost:5000/health" "WebAPI"

Write-Host ""
if (-not $authOk -or -not $apiOk) {
    Write-Host "⚠ ADVERTENCIA: Algunos servicios del backend no están disponibles" -ForegroundColor Yellow
    Write-Host "Asegúrate de ejecutar: docker-compose up -d" -ForegroundColor Yellow
    Write-Host ""
}

# Instrucciones finales
Write-Host "================================" -ForegroundColor Cyan
Write-Host "  ✓ Instalación Completa        " -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pasos siguientes:" -ForegroundColor Yellow
Write-Host "1. Asegúrate de tener el backend corriendo:" -ForegroundColor White
Write-Host "   docker-compose up -d" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Configura CORS en los servicios .NET (ver SETUP.md)" -ForegroundColor White
Write-Host ""
Write-Host "3. Inicia el servidor de desarrollo:" -ForegroundColor White
Write-Host "   cd university-frontend" -ForegroundColor Gray
Write-Host "   npm start" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Abre tu navegador en: http://localhost:4200" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales de prueba:" -ForegroundColor Yellow
Write-Host "  Email:    admin@university.com" -ForegroundColor Gray
Write-Host "  Password: Admin123!" -ForegroundColor Gray
Write-Host ""
Write-Host "Para más información, consulta SETUP.md" -ForegroundColor Cyan
Write-Host ""
