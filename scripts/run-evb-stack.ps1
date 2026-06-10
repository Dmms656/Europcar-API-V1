#Requires -Version 5.1
<#
.SYNOPSIS
  Levanta microservicios + GraphQL gateway + middleware para probar EvB.
.USAGE
  pwsh ./scripts/run-evb-stack.ps1
  pwsh ./scripts/run-evb-stack.ps1 -SkipMicroservices   # solo GraphQL + Middleware
#>
param([switch]$SkipMicroservices)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$europcar = Join-Path $root 'EUROPCAR_V2'

function Load-EnvFile {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return }
    Get-Content $Path | ForEach-Object {
        if ($_ -match '^\s*#') { return }
        if ($_ -match '^\s*$') { return }
        $parts = $_ -split '=', 2
        if ($parts.Length -eq 2) {
            $name = $parts[0].Trim()
            $val = $parts[1].Trim().Trim('"')
            [Environment]::SetEnvironmentVariable($name, $val, "Process")
        }
    }
}

# RabbitMQ
$dockerBin = 'C:\Program Files\Docker\Docker\resources\bin'
if ((Test-Path $dockerBin) -and ($env:Path -notlike "*$dockerBin*")) {
    $env:Path = "$dockerBin;$env:Path"
}
try {
    docker info 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Iniciando RabbitMQ (docker compose)..." -ForegroundColor Cyan
        docker compose -f (Join-Path $root 'docker-compose.yml') up -d
        Start-Sleep -Seconds 3
        Write-Host "[OK] RabbitMQ - UI: http://localhost:15672 (redcar / redcar_dev)" -ForegroundColor Green
    }
} catch {
    Write-Host "[AVISO] Docker no disponible. EvB requiere RabbitMQ en localhost:5672" -ForegroundColor Yellow
}

Load-EnvFile (Join-Path $europcar '.env')
Load-EnvFile (Join-Path $root 'Middleware.RedCar\.env')

if (-not $SkipMicroservices) {
    Write-Host "Lanzando 5 microservicios (ventanas nuevas)..." -ForegroundColor Cyan
    & (Join-Path $europcar 'scripts\run-all.ps1')
    Start-Sleep -Seconds 8
}

$graphqlProj = Join-Path $europcar 'integration\RedCar.Integration.GraphQl\RedCar.Integration.GraphQl.csproj'
$middlewareProj = Join-Path $root 'Middleware.RedCar\src\Middleware.RedCar.Api\Middleware.RedCar.Api.csproj'

Write-Host "Lanzando GraphQL gateway (:5110)..." -ForegroundColor Cyan
$shell = if (Get-Command pwsh -ErrorAction SilentlyContinue) { 'pwsh' } else { 'powershell' }
Start-Process $shell -ArgumentList @(
    '-NoExit', '-Command',
    "cd '$root'; dotnet run --project '$graphqlProj'"
)

Start-Sleep -Seconds 3

Write-Host "Lanzando Middleware (:5200)..." -ForegroundColor Cyan
Start-Process $shell -ArgumentList @(
    '-NoExit', '-Command',
    "cd '$root'; dotnet run --project '$middlewareProj'"
)

Write-Host @"

=== Stack en marcha ===
  RabbitMQ UI     http://localhost:15672
  GraphQL         http://localhost:5110/graphql
  Middleware      http://localhost:5200/swagger
  MS Catálogo     http://localhost:5102/health/live
  MS Reservas     http://localhost:5105/health/live

Prueba rápida GraphQL (PowerShell):
  Invoke-RestMethod -Uri http://localhost:5110/graphql -Method Post -ContentType 'application/json' -Body '{"query":"{ __typename }"}'

Prueba booking (con token JWT):
  POST http://localhost:5200/api/v2/booking/reservas

"@ -ForegroundColor Green
