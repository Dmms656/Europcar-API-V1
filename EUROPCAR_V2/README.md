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

Usa **Web Service → Environment: Docker** (no “Native” ni solo runtime .NET). Con Docker, Render **no** pide *Build Command* ni *Start Command*: el build es `docker build` con tu Dockerfile y el arranque es el `ENTRYPOINT`/`CMD` de la imagen (en nuestros MS: `dotnet …Api.dll` en el puerto **8080**).

**Microservicios RedCar** (Catálogo, Localizaciones, Clientes, Reservas): el contexto de build es la carpeta `EUROPCAR_V2`. En Render, **Root Directory** = `EUROPCAR_V2` y el **Dockerfile path** relativo a esa raíz:

| Servicio | Root Directory | Dockerfile path |
|----------|----------------|-----------------|
| Catálogo | `EUROPCAR_V2` | `microservices/Catalogo/Dockerfile` |
| Localizaciones | `EUROPCAR_V2` | `microservices/Localizaciones/Dockerfile` |
| Clientes | `EUROPCAR_V2` | `microservices/Clientes/Dockerfile` |
| Reservas | `EUROPCAR_V2` | `microservices/Reservas/Dockerfile` |

**Middleware** (`Middleware.RedCar.Api`): contexto = raíz del repositorio. **Root Directory** vacío y **Dockerfile path** = `Dockerfile` (en la raíz del repo).

Imagen escucha en **8080** (`ASPNETCORE_URLS`). Variables: `ConnectionStrings__Default`, `Jwt__*`, `ASPNETCORE_ENVIRONMENT=Production`. Para **Swagger** en producción (tanto microservicios como `Middleware.RedCar.Api`), define `Swagger__Enabled=true` en el Web Service de Render; en local con `Development` no hace falta.

El monolito historico vive en `_legacy/EuropcarRental/` (referencia; el desarrollo activo es middleware + MS).

---

## 7. Paso a paso en Render (por servicio)

**Antes (una sola vez, Supabase):** sigue `db/microservices/SUPABASE.md`: DDLs, seeds, `99_supabase_grants.sql` con tus 5 contraseñas, extensión **pgcrypto** y `search_path` de `ms_seguridad` con schema `extensions` si aplica.

### Común a los cuatro microservicios con Docker

Cada uno es un **Web Service** distinto en Render.

1. **New → Web Service** → conecta el repo de GitHub.
2. **Environment:** **Docker** (no Native .NET).
3. **Root Directory:** `EUROPCAR_V2`
4. **Dockerfile path:** el de la tabla de abajo (relativo a `EUROPCAR_V2`).
5. **Region / plan:** los que prefieras.
6. **Build / Start:** no los rellenes; Docker ya define build y arranque.
7. **Health check path (opcional):** `/health/live`
8. **Variables de entorno** del servicio (mínimo):

| Variable | Valor |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` (suele venir ya en la imagen; puedes omitir si el health pasa) |
| `ConnectionStrings__Default` | Cadena Npgsql completa del **rol** de ese MS (ver fila en la tabla por MS). Usa host `*.pooler.supabase.com`, puerto **6543**, `Username=ms_<rol>.<PROJECT_REF>`, `SSL Mode=Require`, `Trust Server Certificate=true`, y: `Pooling=true;Maximum Pool Size=10;Multiplexing=false;Max Auto Prepare=0;No Reset On Close=true` |
| `Jwt__SecretKey` | Mismo secreto que uses en el middleware (≥32 caracteres recomendado) |
| `Jwt__Issuer` | Ej. `redcar-v2` |
| `Jwt__Audience` | Ej. `redcar-v2-clients` |
| `Swagger__Enabled` | `true` si quieres `/swagger` en producción |

9. **Deploy** y prueba `https://<tu-servicio>.onrender.com/health/live` y `/info`.

**Plantilla de cadena** (sustituye host del **Transaction pooler** que copie Supabase, `PROJECT_REF`, `PASSWORD`, y el prefijo de usuario `ms_*`):

`Host=<POOLER_HOST>;Port=6543;Database=postgres;Username=ms_<nombre>.<PROJECT_REF>;Password=<PASSWORD>;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;Multiplexing=false;Max Auto Prepare=0;No Reset On Close=true;`

---

### Catálogo (`RedCar.Catalogo.Api`)

| Campo Render | Valor |
|---------------|--------|
| Root Directory | `EUROPCAR_V2` |
| Dockerfile path | `microservices/Catalogo/Dockerfile` |
| `ConnectionStrings__Default` | Usuario PostgreSQL **`ms_catalogo.<PROJECT_REF>`** y contraseña del role catálogo |

---

### Localizaciones (`RedCar.Localizaciones.Api`)

| Campo Render | Valor |
|---------------|--------|
| Root Directory | `EUROPCAR_V2` |
| Dockerfile path | `microservices/Localizaciones/Dockerfile` |
| `ConnectionStrings__Default` | Usuario **`ms_localizaciones.<PROJECT_REF>`** |

---

### Clientes (`RedCar.Clientes.Api`)

| Campo Render | Valor |
|---------------|--------|
| Root Directory | `EUROPCAR_V2` |
| Dockerfile path | `microservices/Clientes/Dockerfile` |
| `ConnectionStrings__Default` | Usuario **`ms_clientes.<PROJECT_REF>`** |

---

### Reservas (`RedCar.Reservas.Api`)

| Campo Render | Valor |
|---------------|--------|
| Root Directory | `EUROPCAR_V2` |
| Dockerfile path | `microservices/Reservas/Dockerfile` |
| `ConnectionStrings__Default` | Usuario **`ms_reservas.<PROJECT_REF>`** |

---

### Seguridad (`RedCar.Seguridad.Api`)

En este repo **no hay Dockerfile** bajo `microservices/Seguridad/`; el login y la BD de seguridad van **embebidos en el middleware** (`Middleware.RedCar.Api`) en producción típica.

- **Solo local / laboratorio:** `dotnet run` sobre `RedCar.Seguridad.Api` y variable `ConnectionStrings__Default__Seguridad` (o `ConnectionStrings__Default` en `appsettings`/env).
- **Si más adelante** quisieras Seguridad como contenedor en Render, habría que añadir un `Dockerfile` análogo a los otros cuatro y desplegarlo igual (Root `EUROPCAR_V2`, cadena con **`ms_seguridad.<PROJECT_REF>`**).

---

### Middleware (orquestador + auth)

No está bajo `EUROPCAR_V2/` en el árbol de despliegue.

| Campo Render | Valor |
|---------------|--------|
| Root Directory | *(vacío = raíz del repositorio)* |
| Dockerfile path | `Dockerfile` (en la raíz del repo, junto a `Middleware.RedCar/`) |
| `ConnectionStrings__Default` | Cadena de **`ms_seguridad`** (misma BD Supabase que el seed de seguridad). El middleware aplica ajustes extra si el host es `pooler.supabase.com`. |
| `Microservicios__Catalogo__BaseUrl` | URL pública HTTPS del Web Service Catálogo (sin barra final) |
| `Microservicios__Localizaciones__BaseUrl` | URL de Localizaciones |
| `Microservicios__Clientes__BaseUrl` | URL de Clientes |
| `Microservicios__Reservas__BaseUrl` | URL de Reservas |
| `Jwt__SecretKey` (y Issuer/Audience) | Igual que en los MS para que los tokens emitidos en login se validen al llamar a los MS |
| `Cors__AllowedOrigins__0` | Origen del front (ej. `https://tu-front.onrender.com`) si el navegador llama al middleware |

Tras desplegar los cuatro MS, pega sus URLs públicas en estas variables y redeploy del middleware.
