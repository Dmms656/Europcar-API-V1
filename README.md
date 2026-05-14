# Plataforma RedCar / Europcar (stack actual)

Repositorio: [github.com/Dmms656/Europcar-API-V1](https://github.com/Dmms656/Europcar-API-V1).

Plataforma de renta de vehículos: **frontend** en React + Vite, **backend** distribuido (middleware + microservicios .NET 10) y **scripts SQL** en `db/`.

## Arquitectura del backend (activo)

| Carpeta | Rol |
|--------|-----|
| `Middleware.RedCar/` | API única de cara al cliente: **RedCar V2** (`/api/v2/...`), compat **booking** (`/api/v1/...`), **Auth embebido** (`/api/v1/Auth/...`, mismo proceso) y **stubs** de panel hasta CRUD real en otros MS. |
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

**Auth y panel:** una sola `VITE_API_URL` al middleware. Login/registro se ejecutan **dentro** del middleware contra PostgreSQL (`security.*`). El dashboard sigue usando stubs hasta conectar Catálogo/Clientes/Reservas. Opcional: MS **Seguridad** por separado solo si quieres dividir despliegues.

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

- **Docker**: el `Dockerfile` en la raíz publica **Middleware.RedCar.Api** con **Auth embebido** (incluye código de `EUROPCAR_V2` Seguridad.Business). Render: raíz del repo, Dockerfile `./Dockerfile`. Variables mínimas: `ConnectionStrings__Default`, `Jwt__*`, `Microservicios__*` (Catálogo, Localizaciones, Clientes, Reservas), `Cors__AllowedOrigins__0`.
- **Render (frontend estático)**: `render.yaml`. **`VITE_API_URL`** = URL del middleware + `/api/v1`.

## Documentación adicional

- `EUROPCAR_V2/README.md` — visión de microservicios y fases.
- `db/microservices/README.md` — orden de aplicación de scripts y grants.
- `Contexto/` — documentación funcional (si está presente en tu clon; puede ignorarse en `.gitignore`).

## Licencia

Definir según lineamientos del equipo o del proyecto académico.
