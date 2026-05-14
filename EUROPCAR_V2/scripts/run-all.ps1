# =====================================================
# EUROPCAR_V2 - Levantar los 5 microservicios en paralelo (modo dev)
# =====================================================
# Uso:   pwsh ./EUROPCAR_V2/scripts/run-all.ps1
# Cierra:   ctrl+c en cada ventana o cierra todas las ventanas que se abrieron.
# =====================================================

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $scriptDir

# Cargar el .env como variables de proceso (sobrevive al lanzamiento de hijos)
$envFile = Join-Path $root '.env'
if (-not (Test-Path $envFile)) {
    Write-Error ".env no encontrado en $envFile. Copia .env.example y rellena los valores."
    exit 1
}

Get-Content $envFile | ForEach-Object {
    if ($_ -match '^\s*#') { return }
    if ($_ -match '^\s*$') { return }
    $parts = $_ -split '=', 2
    if ($parts.Length -eq 2) {
        $name = $parts[0].Trim()
        $val  = $parts[1].Trim().Trim('"')
        [Environment]::SetEnvironmentVariable($name, $val, 'Process')
    }
}

Write-Host "Variables de entorno cargadas desde $envFile" -ForegroundColor Green

$services = @(
    @{ Name = 'Seguridad';      Project = 'microservices/Seguridad/RedCar.Seguridad.Api';           Url = 'http://localhost:5101' }
    @{ Name = 'Catalogo';       Project = 'microservices/Catalogo/RedCar.Catalogo.Api';             Url = 'http://localhost:5102' }
    @{ Name = 'Localizaciones'; Project = 'microservices/Localizaciones/RedCar.Localizaciones.Api'; Url = 'http://localhost:5103' }
    @{ Name = 'Clientes';       Project = 'microservices/Clientes/RedCar.Clientes.Api';             Url = 'http://localhost:5104' }
    @{ Name = 'Reservas';       Project = 'microservices/Reservas/RedCar.Reservas.Api';             Url = 'http://localhost:5105' }
)

foreach ($svc in $services) {
    $projPath = Join-Path $root $svc.Project
    Write-Host "Levantando $($svc.Name) en $($svc.Url) ..." -ForegroundColor Cyan
    Start-Process -FilePath 'pwsh' -ArgumentList @(
        '-NoExit',
        '-Command',
        "Set-Location '$projPath'; dotnet run --no-restore -c Debug"
    )
}

Write-Host ""
Write-Host "5 microservicios lanzados. Endpoints de smoke test:" -ForegroundColor Green
foreach ($svc in $services) {
    Write-Host "  GET $($svc.Url)/info" -ForegroundColor Gray
    Write-Host "  GET $($svc.Url)/health/ready" -ForegroundColor Gray
}
