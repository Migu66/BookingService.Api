# Script para crear y aplicar migraciones de PostgreSQL
# Asegúrate de cerrar Visual Studio antes de ejecutar este script

Write-Host "=== BookingService.Api - Migración a PostgreSQL ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que dotnet-ef esté instalado
Write-Host "Verificando herramientas de Entity Framework..." -ForegroundColor Yellow
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "Instalando dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef --version 8.0.11
}

Write-Host "dotnet-ef instalado correctamente" -ForegroundColor Green
Write-Host ""

# Navegar al directorio del proyecto
$projectPath = "BookingService.Api"
Set-Location $projectPath

Write-Host "Limpiando build anterior..." -ForegroundColor Yellow
dotnet clean

Write-Host ""
Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: El proyecto no compila. Por favor revisa los errores anteriores." -ForegroundColor Red
    Write-Host ""
    Write-Host "Posible solución:" -ForegroundColor Yellow
    Write-Host "1. Cierra Visual Studio completamente" -ForegroundColor White
    Write-Host "2. Ejecuta este script nuevamente" -ForegroundColor White
    Write-Host "3. Si persiste el error, ejecuta: dotnet clean" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Creando migración 'InitialPostgreSQL'..." -ForegroundColor Yellow
dotnet ef migrations add InitialPostgreSQL

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: No se pudo crear la migración." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Aplicando migración a la base de datos..." -ForegroundColor Yellow
dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== ¡Migración completada exitosamente! ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "La base de datos PostgreSQL ha sido creada y configurada." -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Cyan
    Write-Host "1. Abre Visual Studio" -ForegroundColor White
    Write-Host "2. Ejecuta el proyecto (F5)" -ForegroundColor White
    Write-Host "3. La API usará la base de datos según el entorno:" -ForegroundColor White
    Write-Host "   - Desarrollo: ep-shiny-hill-abze7d6f-pooler.eu-west-2.aws.neon.tech" -ForegroundColor Gray
    Write-Host "   - Producción: ep-young-voice-abx05zb4-pooler.eu-west-2.aws.neon.tech" -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "ERROR: No se pudo aplicar la migración." -ForegroundColor Red
    Write-Host ""
    Write-Host "Verifica:" -ForegroundColor Yellow
    Write-Host "1. Que las cadenas de conexión en appsettings.json sean correctas" -ForegroundColor White
    Write-Host "2. Que puedas conectarte a la base de datos PostgreSQL" -ForegroundColor White
    Write-Host "3. Que el usuario tenga permisos para crear tablas" -ForegroundColor White
    exit 1
}
