# Event Bus + GraphQL — configuración y pruebas locales

Guía paso a paso para levantar RabbitMQ, microservicios, gateway GraphQL y middleware con saga EvB.

---

## Requisitos

| Componente | Versión / nota |
|------------|----------------|
| .NET SDK | 10.x (`dotnet --version`) |
| Docker Desktop | Engine en estado **Running** |
| PostgreSQL | Supabase ya configurado en `EUROPCAR_V2/.env` |
| Terminal | PowerShell (no hace falta `pwsh`) |

---

## Paso 1 — Preparar el proyecto (una vez)

Desde la raíz del repositorio:

```powershell
cd "C:\Users\medin\source\repos\Proyecto Desarrollo"
powershell -ExecutionPolicy Bypass -File .\scripts\setup-evb-local.ps1 -EnableEvB -EnableGraphQl
```

Esto hace:
- `dotnet restore` + `dotnet build` de toda la solución
- Añade variables EvB/RabbitMQ/GraphQL a `Middleware.RedCar\.env` y `EUROPCAR_V2\.env`

**Importante:** cierra y abre la terminal después de instalar Docker para que `docker` esté en el PATH.

---

## Paso 2 — RabbitMQ (Docker)

```powershell
cd "C:\Users\medin\source\repos\Proyecto Desarrollo"
docker compose up -d
docker ps
```

Debe aparecer el contenedor `redcar-rabbitmq` en estado **Up**.

| Recurso | URL / credenciales |
|---------|-------------------|
| AMQP | `localhost:5672` |
| Management UI | http://localhost:15672 |
| Usuario | `redcar` |
| Contraseña | `redcar_dev` |
| Virtual host | `/redcar-marketplace` |

En la UI de RabbitMQ: **Admin → Virtual Hosts** debe existir `/redcar-marketplace`.

---

## Paso 3 — SQL outbox (Supabase, una vez)

En el **SQL Editor** de Supabase, conexión al proyecto de **Reservas**:

1. Ejecutar: `db/microservices/reservas/03_outbox_inbox.sql`

Opcional (fase 2 clientes):

2. Ejecutar: `db/microservices/clientes/03_outbox_inbox.sql`

Sin estas tablas, el MS Reservas no puede publicar eventos al bus.

---

## Paso 4 — Levantar todos los servicios

### Opción A — Script automático (recomendado)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-evb-stack.ps1
```

Abre ventanas nuevas para microservicios, GraphQL y middleware.

### Opción B — Manual (3–4 terminales)

**Terminal 1 — Microservicios (5101–5105):**

```powershell
cd EUROPCAR_V2
powershell -ExecutionPolicy Bypass -File .\scripts\run-all.ps1
```

**Terminal 2 — GraphQL gateway (5110):**

```powershell
dotnet run --project EUROPCAR_V2\integration\RedCar.Integration.GraphQl
```

**Terminal 3 — Middleware (5200):**

```powershell
dotnet run --project Middleware.RedCar\src\Middleware.RedCar.Api
```

---

## Paso 5 — Verificar que todo responde

```powershell
# Health microservicios
Invoke-RestMethod http://localhost:5101/health/live
Invoke-RestMethod http://localhost:5105/health/live

# GraphQL
$body = '{"query":"{ __typename }"}'
Invoke-RestMethod -Uri http://localhost:5110/graphql -Method Post -ContentType "application/json" -Body $body

# Swagger middleware
Start-Process http://localhost:5200/swagger
```

---

## Paso 6 — Obtener token JWT (opcional)

El booking V2 es `[AllowAnonymous]`, pero para otras rutas:

```powershell
$login = @{
  username = "admin"
  password = "12345"
} | ConvertTo-Json

$r = Invoke-RestMethod -Uri http://localhost:5101/api/v1/Auth/login `
  -Method Post -ContentType "application/json" -Body $login

$token = $r.data.token
Write-Host "Token: $token"
```

Usuarios demo (seed): `admin` / `12345`, `cliente.carlos` / `12345`.

---

## Paso 7 — Probar disponibilidad (GraphQL activo)

Con `Integration__UseGraphQl=true` en el middleware:

```powershell
$fechaInicio = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:sszzz")
$fechaFin    = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:sszzz")

Invoke-RestMethod "http://localhost:5200/api/v2/booking/reservas/1/disponibilidad?idLocalizacion=1&fechaRecogida=$fechaInicio&fechaDevolucion=$fechaFin"
```

---

## Paso 8 — Probar crear reserva vía Event Bus

Con `EvB__Enabled=true` el `POST` dispara la saga RabbitMQ (misma respuesta HTTP que el flujo síncrono).

```powershell
$payload = @{
  idVehiculo = 1
  idLocalizacionRecogida = 1
  idLocalizacionDevolucion = 1
  fechaInicio = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
  fechaFin    = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
  horaInicio  = "10:00:00"
  horaFin     = "10:00:00"
  cliente = @{
    nombres = "Juan"
    apellidos = "Pérez"
    tipoIdentificacion = "CEDULA"
    numeroIdentificacion = "1712345678"
    correo = "juan@test.com"
    telefono = "0999999999"
  }
  conductores = @(
    @{
      nombres = "Juan"
      apellidos = "Pérez"
      tipoIdentificacion = "CEDULA"
      numeroIdentificacion = "1712345678"
      fechaVencimientoLicencia = "2030-12-31"
      edadConductor = 30
      correo = "juan@test.com"
      telefono = "0999999999"
      esPrincipal = $true
    }
  )
  extras = @()
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri http://localhost:5200/api/v2/booking/reservas `
  -Method Post -ContentType "application/json" -Body $payload
```

**Qué observar en RabbitMQ (http://localhost:15672):**
- Colas de MassTransit con prefijo del servicio
- Mensajes `ProcesarReservaBookingCommand` y eventos `ReservaCreada`
- En Supabase: filas en `reservas.outbox_messages` que pasan a `published_at` no nulo

---

## Flags de configuración

| Variable | Ubicación | Efecto |
|----------|-----------|--------|
| `EvB__Enabled=false` | `Middleware.RedCar\.env` | Flujo síncrono clásico (default) |
| `EvB__Enabled=true` | idem | Saga async vía RabbitMQ |
| `Integration__UseGraphQl=true` | idem | Disponibilidad vía `:5110/graphql` |
| `RabbitMQ__*` | Middleware + EUROPCAR_V2 `.env` | Conexión MassTransit |

Para volver al modo clásico sin tocar código:

```env
EvB__Enabled=false
Integration__UseGraphQl=false
```

Reinicia middleware y microservicios tras cambiar `.env`.

---

## Puertos de referencia

| Servicio | Puerto |
|----------|--------|
| Seguridad | 5101 |
| Catálogo | 5102 |
| Localizaciones | 5103 |
| Clientes | 5104 |
| Reservas | 5105 |
| GraphQL gateway | 5110 |
| Middleware | 5200 |
| RabbitMQ AMQP | 5672 |
| RabbitMQ UI | 15672 |

---

## Solución de problemas

**`docker` no reconocido**  
Cierra Cursor/terminal y ábrela de nuevo. O usa ruta completa:
`& "C:\Program Files\Docker\Docker\resources\bin\docker.exe" compose up -d`

**Error 500 al ejecutar `docker compose` desde Cursor**  
Ejecuta los comandos Docker en **PowerShell externo** (donde iniciaste sesión en Docker Desktop).

**MS no conecta a RabbitMQ**  
Verifica que RabbitMQ esté Up y que `RabbitMQ__VirtualHost=/redcar-marketplace` esté en ambos `.env`.

**Timeout en crear reserva con EvB**  
Revisa colas en RabbitMQ, logs de Reservas/Clientes/Middleware, y que el SQL outbox esté aplicado.

**Volver a modo síncrono**  
`EvB__Enabled=false` + reiniciar servicios.
