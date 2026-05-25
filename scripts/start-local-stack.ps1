# Levanta microservicios EUROPCAR_V2 + Middleware.RedCar (y opcionalmente monolito legacy).
param(
    [switch]$Legacy,
    [switch]$MiddlewareOnly
)

$ErrorActionPreference = "Stop"
$repo = Split-Path $PSScriptRoot -Parent
$v2 = Join-Path $repo "EUROPCAR_V2"
$mw = Join-Path $repo "Middleware.RedCar"

function Import-DotEnv([string]$path) {
    if (-not (Test-Path $path)) { Write-Warning "No .env: $path"; return }
    Get-Content $path | ForEach-Object {
        if ($_ -match '^\s*#' -or $_ -match '^\s*$') { return }
        $p = $_ -split '=', 2
        if ($p.Length -eq 2) {
            [Environment]::SetEnvironmentVariable($p[0].Trim(), $p[1].Trim().Trim('"'), 'Process')
        }
    }
}

function Start-Api([string]$name, [string]$project, [string]$url) {
    $proj = Join-Path $v2 $project
    Write-Host "  -> $name $url" -ForegroundColor Cyan
    Start-Process pwsh -ArgumentList @(
        '-NoExit', '-Command',
        @"
Set-Location '$v2'
function Import-DotEnv(`$path) {
  Get-Content `$path | ForEach-Object {
    if (`$_ -match '^\s*#' -or `$_ -match '^\s*$') { return }
    `$p = `$_ -split '=', 2
    if (`$p.Length -eq 2) { [Environment]::SetEnvironmentVariable(`$p[0].Trim(), `$p[1].Trim().Trim('"'), 'Process') }
  }
}
Import-DotEnv '.env'
`$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project '$proj' --urls '$url'
"@
    ) -WindowStyle Minimized
}

Import-DotEnv (Join-Path $v2 ".env")

if (-not $MiddlewareOnly) {
    Write-Host "`nMicroservicios:" -ForegroundColor Green
    Start-Api "Catalogo" "microservices/Catalogo/RedCar.Catalogo.Api/RedCar.Catalogo.Api.csproj" "http://localhost:5102"
    Start-Api "Localizaciones" "microservices/Localizaciones/RedCar.Localizaciones.Api/RedCar.Localizaciones.Api.csproj" "http://localhost:5103"
    Start-Api "Clientes" "microservices/Clientes/RedCar.Clientes.Api/RedCar.Clientes.Api.csproj" "http://localhost:5104"
    Start-Api "Reservas" "microservices/Reservas/RedCar.Reservas.Api/RedCar.Reservas.Api.csproj" "http://localhost:5105"
    Start-Sleep -Seconds 8
}

Write-Host "`nMiddleware.RedCar:" -ForegroundColor Green
$mwProj = Join-Path $mw "src/Middleware.RedCar.Api/Middleware.RedCar.Api.csproj"
Import-DotEnv (Join-Path $mw ".env")
Start-Process pwsh -ArgumentList @(
    '-NoExit', '-Command',
    @"
Set-Location '$mw'
function Import-DotEnv(`$path) {
  Get-Content `$path | ForEach-Object {
    if (`$_ -match '^\s*#' -or `$_ -match '^\s*$') { return }
    `$p = `$_ -split '=', 2
    if (`$p.Length -eq 2) { [Environment]::SetEnvironmentVariable(`$p[0].Trim(), `$p[1].Trim().Trim('"'), 'Process') }
  }
}
Import-DotEnv '.env'
Import-DotEnv '$v2\.env'
`$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project '$mwProj' --urls 'http://localhost:5200'
"@
) -WindowStyle Minimized

if ($Legacy) {
    Write-Host "`nMonolito legacy (5207):" -ForegroundColor Green
    $leg = Join-Path $repo "_legacy\EuropcarRental\src\Europcar.Rental.Api\Europcar.Rental.Api.csproj"
    Start-Process pwsh -ArgumentList @(
        '-NoExit', '-Command',
        "`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --project '$leg' --urls 'http://localhost:5207'"
    ) -WindowStyle Minimized
}

Write-Host "`nURLs:" -ForegroundColor Yellow
Write-Host "  Middleware + Swagger: http://localhost:5200/swagger"
if (-not $MiddlewareOnly) {
    Write-Host "  MS Catalogo:       http://localhost:5102/swagger"
    Write-Host "  MS Localizaciones: http://localhost:5103/swagger"
    Write-Host "  MS Clientes:       http://localhost:5104/swagger"
    Write-Host "  MS Reservas:       http://localhost:5105/swagger"
}
if ($Legacy) { Write-Host "  Monolito legacy:   http://localhost:5207/swagger" }
Write-Host "`nEspera ~30s en cold start y prueba /health/ready en cada uno." -ForegroundColor Gray
