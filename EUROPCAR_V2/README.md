# EUROPCAR V2 - Microservicios

Implementacion de la arquitectura **RedCar V2** con 5 microservicios independientes
y el orquestador en `Middleware.RedCar/` (raiz del repo).

> Antes de empezar, leer `db/microservices/SUPABASE.md` para tener las 5 bases
> de datos creadas en Supabase con los roles `ms_*` y los GRANTs correctos.

---

## 1. Microservicios

| MS | Schema | Connection string (env) | Puerto REST | Puerto gRPC | Url base |
|---|---|---|---|---|---|
| `RedCar.Seguridad` | `security` + `audit` | `ConnectionStrings__Default__Seguridad` | 5101 | 5101 (H2) | `http://localhost:5101` |
| `RedCar.Catalogo` | `catalogo` | `ConnectionStrings__Default__Catalogo` | 5102 | 5102 (H2) | `http://localhost:5102` |
| `RedCar.Localizaciones` | `localizaciones` | `ConnectionStrings__Default__Localizaciones` | 5103 | 5103 (H2) | `http://localhost:5103` |
| `RedCar.Clientes` | `clientes` | `ConnectionStrings__Default__Clientes` | 5104 | 5104 (H2) | `http://localhost:5104` |
| `RedCar.Reservas` | `reservas` | `ConnectionStrings__Default__Reservas` | 5105 | 5105 (H2) | `http://localhost:5105` |

Cada microservicio expone **REST + gRPC en el mismo proceso** sobre HTTP/2 (Kestrel),
asi que ambos comparten puerto.

---

## 2. Estructura por microservicio (Clean Architecture)

```
microservices/<Dominio>/
├── RedCar.<Dominio>.Api               <- REST controllers + gRPC services + JWT + Swagger
├── RedCar.<Dominio>.Business          <- Casos de uso, orquestaciones internas, validadores, DTOs
├── RedCar.<Dominio>.DataManagement    <- Coordinadores transaccionales y lógica multi-FK interna
└── RedCar.<Dominio>.DataAccess        <- DbContext, entidades y repositorios (un solo schema)
```

Reglas:
- `Api` depende de `Business`.
- `Business` depende de `DataManagement`.
- `DataManagement` depende de `DataAccess`.
- `DataAccess` solo conoce su schema. Nunca cruza a otro microservicio.

---

## 3. Proyectos compartidos (`shared/`)

| Proyecto | Para que |
|---|---|
| `RedCar.Shared.Contracts` | DTOs y tipos comunes: `Result<T>`, `ApiResponse<T>`, `PagedResult<T>`, `ErrorCodes` |
| `RedCar.Shared.Protos` | Archivos `.proto` con los contratos gRPC de cada microservicio |
| `RedCar.Shared.Auth` | `JwtSettings`, `AddRedCarJwt(...)`, constantes de policies y roles |

---

## 4. Variables de entorno

Copia `.env.example` a `.env` (gitignored) y rellena los placeholders.

Las variables se leen automaticamente por ASP.NET Core via *Environment Variables Configuration Provider*:
`ConnectionStrings__Default__Seguridad` -> `Configuration["ConnectionStrings:Default:Seguridad"]`.

Cada `Program.cs` selecciona la subllave que corresponde a su microservicio.

### Cargar el .env en una sesion de desarrollo

PowerShell:
```powershell
Get-Content EUROPCAR_V2/.env | ForEach-Object {
    if ($_ -match '^\s*#') { return }
    if ($_ -match '^\s*$') { return }
    $parts = $_ -split '=', 2
    if ($parts.Length -eq 2) {
        $name = $parts[0].Trim()
        $val  = $parts[1].Trim().Trim('"')
        [Environment]::SetEnvironmentVariable($name, $val, 'Process')
    }
}
```

Una vez cargado, `dotnet run --project ...` recoge las variables.

---

## 5. Levantar localmente un microservicio

```powershell
dotnet build EUROPCAR_V2/EUROPCAR_V2.sln

# Seguridad
dotnet run --project EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.Api

# Catalogo
dotnet run --project EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Api

# Localizaciones, Clientes, Reservas: idem
```

Endpoints disponibles por MS:
- `GET /info` -> nombre, version, schema, status
- `GET /health/live` -> proceso vivo
- `GET /health/ready` -> dependencias OK (DB)
- `GET /swagger` -> documentacion OpenAPI (solo dev)
- gRPC services (mismo puerto, HTTP/2)

---

## 6. Fases

- [x] **Fase 0** - Bases de datos en Supabase (`db/microservices/`).
- [x] **Fase 1** - Esqueleto de los 5 microservicios (esta entrega). Cada MS compila,
      arranca, expone `/info` y `/health`, y se conecta a su schema en Supabase.
- [ ] **Fase 2** - Entidades, repositorios y servicios CRUD por microservicio (uno a uno).
- [ ] **Fase 3** - Contratos gRPC reales (.proto) y server implementation.
- [ ] **Fase 4** - JWT login en MS.Seguridad + validacion en los otros.
- [x] **Fase 5** - Orquestador `Middleware.RedCar.Api` (HttpClients + gRPC hacia los MS).
- [ ] **Fase 6** - Despliegue en Render / contenedores (ver tabla Docker abajo).

### Render sin runtime .NET nativo

Usa **Web Service → Language: Docker**.

**Microservicios RedCar** (Catálogo, Localizaciones, Clientes, Reservas): el contexto de build es la carpeta `EUROPCAR_V2`. En Render, **Root Directory** = `EUROPCAR_V2` y el **Dockerfile path** relativo a esa raíz:

| Servicio | Root Directory | Dockerfile path |
|----------|----------------|-----------------|
| Catálogo | `EUROPCAR_V2` | `microservices/Catalogo/Dockerfile` |
| Localizaciones | `EUROPCAR_V2` | `microservices/Localizaciones/Dockerfile` |
| Clientes | `EUROPCAR_V2` | `microservices/Clientes/Dockerfile` |
| Reservas | `EUROPCAR_V2` | `microservices/Reservas/Dockerfile` |

**Middleware** (`Middleware.RedCar.Api`): contexto = raíz del repositorio. **Root Directory** vacío y **Dockerfile path** = `Dockerfile` (en la raíz del repo).

Imagen escucha en **8080** (`ASPNETCORE_URLS`). Variables: `ConnectionStrings__Default`, `Jwt__*`, `ASPNETCORE_ENVIRONMENT=Production`.

El monolito historico vive en `_legacy/EuropcarRental/` (referencia; el desarrollo activo es middleware + MS).
