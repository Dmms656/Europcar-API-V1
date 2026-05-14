# RedCar V2 — Desplegar microservicios en Render y conectar el Middleware

Guía única: **Supabase + GitHub + Render** (Web Services con **Docker**). El auth de login va **embebido en el middleware**; no hay contenedor separado de Seguridad en este repo.

---

## 0. Prerrequisitos

| Requisito | Nota |
|-----------|------|
| Cuenta [Render](https://render.com) y [Supabase](https://supabase.com) | Mismo proyecto Supabase para los 5 schemas |
| Repo en **GitHub** enlazado a Render | El build clona desde ahí |
| DDL + seeds + roles aplicados en Supabase | Ver sección 1 |

---

## 1. Base de datos (Supabase) — una sola vez por proyecto

Sigue en detalle: [`db/microservices/SUPABASE.md`](../db/microservices/SUPABASE.md).

Resumen del orden:

1. Ejecutar los **5 DDL** (`01_ddl.sql` por dominio) en el SQL Editor.
2. Ejecutar los **5 seeds** (`02_seed.sql`) en el orden indicado en `SUPABASE.md`.
3. Ejecutar **`db/microservices/99_supabase_grants.sql`** (sustituye antes los 5 placeholders de contraseña por valores fuertes; no commitees contraseñas reales).
4. **pgcrypto + `ms_seguridad`**: si el login fallaba con `42883 function crypt...`, aplica el bloque SQL de `SUPABASE.md` (extensión + `search_path` con schema `extensions` si aplica).
5. **RLS**: si en `security`/`audit` ves `relrowsecurity = true` en las tablas, ejecuta **`db/microservices/seguridad/03_disable_rls_for_service_roles.sql`** (los roles `ms_*` no ven filas con RLS sin políticas; el login responde “credenciales inválidas”).
6. Anota: **host del pooler transaccional** (puerto **6543**), **`PROJECT_REF`**, y las **5 contraseñas** de los roles `ms_*`.

Plantilla Npgsql recomendada (ajusta host, usuario `ms_<rol>.<PROJECT_REF>` y contraseña):

```text
Host=<POOLER_HOST>;Port=6543;Database=postgres;Username=ms_<rol>.<PROJECT_REF>;Password=<PASSWORD>;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;Multiplexing=false;Max Auto Prepare=0;No Reset On Close=true;
```

Referencia de variables locales: [`EUROPCAR_V2/.env.example`](../EUROPCAR_V2/.env.example).

---

## 2. Crear cada microservicio en Render (Catálogo, Localizaciones, Clientes, Reservas)

`RedCar.Seguridad.Api` **no tiene Dockerfile** en este monorepo; el login usa la BD de seguridad **dentro del middleware**. Los cuatro siguientes sí se despliegan cada uno como **Web Service** propio.

### 2.1 Pasos comunes (repetir por cada MS)

1. Render → **New** → **Web Service** → conecta el repositorio GitHub.
2. **Environment**: **Docker** (no “Native” .NET). No hace falta *Build Command* ni *Start Command*.
3. **Root Directory**: `EUROPCAR_V2`
4. **Dockerfile path**: según la tabla de abajo (ruta relativa a `EUROPCAR_V2`).
5. **Instance type / region**: según necesidad.
6. **Health Check** (opcional): path **`/health/live`**.
7. **Environment** → variables (sección 2.2).
8. **Create Web Service** y esperar el primer deploy.
9. Anota la URL pública **`https://<nombre>.onrender.com`** (sin barra final); la usarás en el middleware.

### 2.2 Variables de entorno (cada Web Service de microservicio)

| Variable | Valor |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | Cadena completa del **rol de ese microservicio** (`ms_catalogo`, `ms_localizaciones`, `ms_clientes` o `ms_reservas`). Misma plantilla de la sección 1. |
| `Jwt__SecretKey` | Secreto largo (≥32 caracteres recomendado). **Debe ser el mismo** en los cuatro MS y en el middleware. |
| `Jwt__Issuer` | Ej. `redcar-v2` (igual en todos). |
| `Jwt__Audience` | Ej. `redcar-v2-clients` (igual en todos). |
| `Swagger__Enabled` | `true` si quieres documentación en **`/swagger`** en producción; si no, omítela o `false`. |

La imagen Docker ya expone **`8080`** (`ASPNETCORE_URLS`); Render suele inyectar `PORT`; si el health falla, revisa logs.

### 2.3 Tabla por microservicio

| Microservicio | Dockerfile path (desde `EUROPCAR_V2/`) | Usuario en `ConnectionStrings__Default` |
|---------------|----------------------------------------|------------------------------------------|
| **Catálogo** | `microservices/Catalogo/Dockerfile` | `ms_catalogo.<PROJECT_REF>` |
| **Localizaciones** | `microservices/Localizaciones/Dockerfile` | `ms_localizaciones.<PROJECT_REF>` |
| **Clientes** | `microservices/Clientes/Dockerfile` | `ms_clientes.<PROJECT_REF>` |
| **Reservas** | `microservices/Reservas/Dockerfile` | `ms_reservas.<PROJECT_REF>` |

### 2.4 Comprobación rápida por MS

- `GET https://<tu-ms>.onrender.com/health/live` → proceso vivo.
- `GET https://<tu-ms>.onrender.com/health/ready` → base de datos (si la cadena es correcta).
- `GET https://<tu-ms>.onrender.com/info` → metadatos del servicio.
- Si activaste Swagger: `GET .../swagger`.

---

## 3. Crear el Middleware en Render

1. **New** → **Web Service** → mismo repo GitHub.
2. **Environment**: **Docker**.
3. **Root Directory**: **vacío** (raíz del repositorio, no `EUROPCAR_V2`).
4. **Dockerfile path**: `Dockerfile` (el de la **raíz** del monorepo, junto a `Middleware.RedCar/` y `EUROPCAR_V2/`).
5. **Health check** (opcional): `/health/live`.
6. Variables → sección 4.
7. Deploy. El middleware aplica solo a cadenas al **pooler Supabase** ajustes `Multiplexing` / `NoResetOnClose` / `MaxAutoPrepare` cuando el host es `pooler.supabase.com` **o** el puerto es **6543**.

---

## 4. Variables del Middleware cuando los MS ya están en línea

Sustituye las URLs por las **HTTPS públicas** de cada Web Service (sin `/` final).

| Variable | Descripción |
|----------|-------------|
| `ConnectionStrings__Default` | Cadena Npgsql del rol **`ms_seguridad.<PROJECT_REF>`** (misma BD y convención que en `.env.example` de seguridad). Aquí vive el login embebido. |
| `Microservicios__Catalogo__BaseUrl` | `https://<catalogo>.onrender.com` |
| `Microservicios__Localizaciones__BaseUrl` | `https://<localizaciones>.onrender.com` |
| `Microservicios__Clientes__BaseUrl` | `https://<clientes>.onrender.com` |
| `Microservicios__Reservas__BaseUrl` | `https://<reservas>.onrender.com` |
| `Jwt__SecretKey` | **Igual** que en los cuatro microservicios (firma de tokens en login y validación en downstream). |
| `Jwt__Issuer` | Igual que en los MS. |
| `Jwt__Audience` | Igual que en los MS. |
| `Swagger__Enabled` | `true` si quieres **`/swagger`** en producción. |
| `Cors__AllowedOrigins__0` | Origen del **frontend** (ej. `https://tu-front.onrender.com`). Para más orígenes: `Cors__AllowedOrigins__1`, etc. |

Opcionales útiles:

| Variable | Uso |
|----------|-----|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Microservicios__Catalogo__TimeoutSeconds` | Por defecto suele bastar 10; Reservas a veces 30. |

Referencia local: [`Middleware.RedCar/.env.example`](../Middleware.RedCar/.env.example).

### 4.1 Orden recomendado

1. Desplegar y verificar los **cuatro** microservicios.
2. Copiar sus URLs públicas.
3. Crear o **actualizar** el Web Service del **middleware** con `Microservicios__*__BaseUrl` y redeploy.
4. Probar **`POST /api/v1/Auth/login`** en el middleware (`username` / `password` en JSON camelCase, usuario seed ej. `admin` / `12345` si aplicaste `seguridad/02_seed.sql`).
5. Probar una ruta del middleware que llame a un MS (con Bearer si aplica).

---

## 5. Checklist final

- [ ] Supabase: DDL + seeds + `99_supabase_grants.sql`
- [ ] Supabase: sin RLS bloqueando a `ms_*` en `security`/`audit` (script `03_...` si hace falta)
- [ ] Supabase: pgcrypto + `search_path` de `ms_seguridad` si hubo error `crypt`
- [ ] Render: 4 Web Services Docker (`Root` = `EUROPCAR_V2`, Dockerfile correcto)
- [ ] Render: cada MS con `ConnectionStrings__Default` del rol correcto + JWT alineado
- [ ] Render: middleware con `Dockerfile` en raíz + `ConnectionStrings__Default` (`ms_seguridad`) + **4 BaseUrl** + JWT + CORS
- [ ] Login OK y front puede llamar al dominio del middleware con CORS

---

## 6. Documentación relacionada

| Documento | Contenido |
|-------------|------------|
| [`db/microservices/SUPABASE.md`](../db/microservices/SUPABASE.md) | Supabase paso a paso, troubleshooting (crypt, stream, RLS) |
| [`EUROPCAR_V2/README.md`](../EUROPCAR_V2/README.md) | Tabla de puertos locales, variables `.env`, sección Render resumida |
| [`db/microservices/README.md`](../db/microservices/README.md) | Árbol de scripts SQL |

Si más adelante quieres **Seguridad como microservicio contenedor**, habría que añadir un `Dockerfile` bajo `EUROPCAR_V2/microservices/Seguridad/` y registrar otro Web Service; el middleware dejaría de usar auth embebido o se reconfiguraría para HTTP hacia ese host.
