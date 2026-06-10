#Requires -Version 5.1
<#
.SYNOPSIS
  Prepara variables de entorno y verifica prerequisitos para Event Bus + GraphQL.
.USAGE
  pwsh ./scripts/setup-evb-local.ps1
  pwsh ./scripts/setup-evb-local.ps1 -EnableEvB -EnableGraphQl
#>
param(
    [switch]$EnableEvB,
    [switch]$EnableGraphQl
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

function Ensure-EnvBlock {
    param([string]$Path, [string[]]$Lines)
    if (-not (Test-Path $Path)) {
        Write-Host "Creando $Path desde .env.example..." -ForegroundColor Yellow
        $example = $Path -replace '\.env$', '.env.example'
        if (Test-Path $example) { Copy-Item $example $Path } else { New-Item -ItemType File -Path $Path | Out-Null }
    }
    $content = Get-Content $Path -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { $content = '' }
    foreach ($line in $Lines) {
        if ($line -match '^\s*$' -or $line -match '^\s*#') { continue }
        $key = ($line -split '=', 2)[0].Trim()
        if ($content -notmatch "(?m)^$([regex]::Escape($key))=") {
            Add-Content -Path $Path -Value $line
            Write-Host "  + $key" -ForegroundColor Green
        }
    }
}

Write-Host "`n=== RedCar EvB + GraphQL - setup local ===" -ForegroundColor Cyan

# 1. .NET
try {
    $dn = dotnet --version
    Write-Host "[OK] .NET SDK $dn" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Instala .NET 10 SDK: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# 2. Docker (añadir al PATH si Desktop está instalado pero la terminal es antigua)
$dockerBin = 'C:\Program Files\Docker\Docker\resources\bin'
if ((Test-Path $dockerBin) -and ($env:Path -notlike "*$dockerBin*")) {
    $env:Path = "$dockerBin;$env:Path"
}

$dockerOk = $false
try {
    $dv = docker --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] $dv" -ForegroundColor Green
        $dockerOk = $true
    }
} catch { }

if (-not $dockerOk) {
    Write-Host "[PENDIENTE] Docker no detectado en PATH." -ForegroundColor Yellow
    Write-Host "  Instala Docker Desktop y reinicia la terminal:" -ForegroundColor Yellow
    Write-Host "  winget install -e --id Docker.DockerDesktop" -ForegroundColor Gray
    Write-Host "  Tras instalar: abre Docker Desktop, espera 'Engine running', luego:" -ForegroundColor Gray
    Write-Host "  docker compose -f `"$root\docker-compose.yml`" up -d" -ForegroundColor Gray
}

# 3. Restore + build
Write-Host "`nRestaurando paquetes NuGet..." -ForegroundColor Cyan
Push-Location $root
dotnet restore RedCar.Platform.slnx | Out-Null
dotnet build RedCar.Platform.slnx -c Release --no-restore
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location
Write-Host "[OK] Build Release correcto" -ForegroundColor Green

# 4. Variables EvB en .env
$evbEnabled = if ($EnableEvB) { 'true' } else { 'false' }
$gqlEnabled = if ($EnableGraphQl) { 'true' } else { 'false' }

$rabbitBlock = @(
    ''
    '# --- Event Bus (RabbitMQ) ---'
    "EvB__Enabled=$evbEnabled"
    'EvB__SagaTimeoutSeconds=90'
    'RabbitMQ__Host=localhost'
    'RabbitMQ__Port=5672'
    'RabbitMQ__VirtualHost=/redcar-marketplace'
    'RabbitMQ__Username=redcar'
    'RabbitMQ__Password=redcar_dev'
    ''
    '# --- Gateway GraphQL (:5110) ---'
    "Integration__UseGraphQl=$gqlEnabled"
    'Integration__GraphQlBaseUrl=http://localhost:5110/graphql'
)

$msRabbit = @(
    ''
    '# --- RabbitMQ (MassTransit) ---'
    'RabbitMQ__Host=localhost'
    'RabbitMQ__Port=5672'
    'RabbitMQ__VirtualHost=/redcar-marketplace'
    'RabbitMQ__Username=redcar'
    'RabbitMQ__Password=redcar_dev'
)

Write-Host "`nActualizando archivos .env..." -ForegroundColor Cyan
Ensure-EnvBlock -Path (Join-Path $root 'Middleware.RedCar\.env') -Lines $rabbitBlock
Ensure-EnvBlock -Path (Join-Path $root 'EUROPCAR_V2\.env') -Lines $msRabbit

# 5. Cargar .env en sesion actual (middleware)
$mwEnv = Join-Path $root 'Middleware.RedCar\.env'
Get-Content $mwEnv | ForEach-Object {
    if ($_ -match '^\s*#') { return }
    if ($_ -match '^\s*$') { return }
    $parts = $_ -split '=', 2
    if ($parts.Length -eq 2) {
        $name = $parts[0].Trim()
        $val = $parts[1].Trim().Trim('"')
        [Environment]::SetEnvironmentVariable($name, $val, "Process")
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "EvB__Enabled              = $evbEnabled"
Write-Host "Integration__UseGraphQl   = $gqlEnabled"
Write-Host ""
Write-Host "Siguiente paso (con Docker activo):" -ForegroundColor White
Write-Host "  docker compose -f `"$root\docker-compose.yml`" up -d" -ForegroundColor Gray
Write-Host "  powershell -File ./scripts/run-evb-stack.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "SQL outbox (Supabase, una vez):" -ForegroundColor White
Write-Host "  db/microservices/reservas/03_outbox_inbox.sql" -ForegroundColor Gray
Write-Host ""
