# Plataforma RedCar / Europcar (stack actual)

Plataforma de renta de vehículos: **frontend** en React + Vite, **backend** distribuido (middleware + microservicios .NET 10) y **scripts SQL** en `db/`.

## Arquitectura del backend (activo)

| Carpeta | Rol |
|--------|-----|
| `Middleware.RedCar/` | API de cara al cliente: contrato **RedCar V2** (`/api/v2/...`) y compatibilidad con el frontend existente (`/api/v1/...`, mismo envelope que el monolito). |
| `EUROPCAR_V2/` | Microservicios (Seguridad, Catálogo, Localizaciones, Clientes, Reservas) y proyectos compartidos (`shared/`). |
| `db/microservices/` | DDL/seed y notas de despliegue de bases por dominio. |
| `_legacy/EuropcarRental/` | Monolito ASP.NET original (solo referencia o funciones aún no portadas; ver `_legacy/README.md`). |

**Solución unificada** (compilar todo el backend activo):

```bash
dotnet restore RedCar.Platform.slnx
dotnet build RedCar.Platform.slnx -c Release
```

**Solo el orquestador** (desarrollo local típico del booking):

```bash
dotnet run --project Middleware.RedCar/src/Middleware.RedCar.Api
```

Por defecto el middleware escucha en **http://localhost:5200** (ver `Middleware.RedCar/src/Middleware.RedCar.Api/Properties/launchSettings.json`). Swagger: `/swagger`. Health: `GET /health/live`, `GET /health/ready`.

### Frontend y URL de API

El cliente Axios usa `VITE_API_URL` como `baseURL`. Para el flujo **booking** contra el middleware debe incluir el prefijo de versión, por ejemplo:

`VITE_API_URL=http://localhost:5200/api/v1`

Detalle: las rutas **admin** (`/api/v1/admin/...`) y **Auth** del monolito **no** están en `Middleware.RedCar`; si necesitas panel interno completo, ejecuta el proyecto legado en `_legacy/EuropcarRental` en otro puerto y apunta allí, o migra esos endpoints al nuevo stack.

## Requisitos previos

- .NET SDK 10
- Node.js 20+ y npm
- PostgreSQL (o Supabase) según scripts en `db/microservices/`

## Frontend

```bash
cd frontend
npm ci
npm run dev
```

Vite suele usar `http://localhost:5173`. Copia `frontend/.env.example` a `frontend/.env` y ajusta URLs.

## Despliegue

- **Docker**: el `Dockerfile` en la raíz publica **Middleware.RedCar.Api** (imagen lista para Render u otro host que exponga el puerto 8080).
- **Render (frontend estático)**: `render.yaml` (build del SPA y rewrites).

## Documentación adicional

- `EUROPCAR_V2/README.md` — visión de microservicios y fases.
- `db/microservices/README.md` — orden de aplicación de scripts y grants.
- `Contexto/` — documentación funcional (si está presente en tu clon; puede ignorarse en `.gitignore`).

## Licencia

Definir según lineamientos del equipo o del proyecto académico.
