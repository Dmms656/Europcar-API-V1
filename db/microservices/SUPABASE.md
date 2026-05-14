# Subir las 5 "bases" a Supabase

Esta guía explica paso a paso cómo desplegar el modelo de datos
de los 5 microservicios en **un único proyecto Supabase** usando
**Opción B**: 1 BD física, 5 schemas, 5 roles DB con aislamiento
por `GRANT` (lo enforcea PostgreSQL, no el código).

> Por qué un solo proyecto: ver `db/microservices/README.md`.
> En resumen, Supabase = 1 BD por proyecto, y el free tier
> sólo permite 2 proyectos activos (se pausan tras 7 días).

Los DDL declaran las tablas en **minúsculas** (`localizaciones.paises`, etc.),
alineado con cómo PostgreSQL y el Explorador de Supabase muestran los
objetos. Evita escribir `"PAISES"` entre comillas: eso crearía otro objeto
distinto de `paises`.

---

## 0. Pre-requisitos

- Cuenta en [supabase.com](https://supabase.com) (gratis).
- Los archivos de `db/microservices/` en tu repo.
- Un gestor de contraseñas (vas a generar 5 password fuertes).
- ~10 minutos.

---

## 1. Crear el proyecto Supabase

1. Entra a [app.supabase.com](https://app.supabase.com).
2. **New project**.
3. Datos:
   - **Name**: `redcar-v2` (o el que prefieras).
   - **Database Password**: genera una **muy fuerte** y guárdala (es la del role `postgres`, no se usa en los microservicios pero la vas a necesitar para administrar la BD).
   - **Region**: la más cercana a donde corra tu backend (ej. `South America (São Paulo)` si despliegas en Render Brazil, o `US East` si despliegas en US).
   - **Pricing Plan**: Free.
4. Espera ~2 minutos a que se aprovisione.

Cuando termine, anota dos cosas que vas a usar más adelante:
- **Project Ref** (parte del subdominio: `https://app.supabase.com/project/<PROJECT_REF>` → ej. `abcdefghijklmnop`).
- **Region** (de la URL del pooler, suele ser algo como `us-east-1`).

Las dos las encuentras en **Project Settings → Database → Connection info**.

---

## 2. Generar 5 contraseñas para los roles de servicio

Cada microservicio tendrá su **propio usuario de base de datos**, separado del `postgres` administrativo. Genera 5 contraseñas fuertes (16+ caracteres):

| Role | Variable a llenar en `.env` |
|---|---|
| `ms_seguridad` | `<PASSWORD_SEGURIDAD>` |
| `ms_catalogo` | `<PASSWORD_CATALOGO>` |
| `ms_localizaciones` | `<PASSWORD_localizaciones>` |
| `ms_clientes` | `<PASSWORD_clientes>` |
| `ms_reservas` | `<PASSWORD_reservas>` |

Guárdalas en tu gestor de contraseñas. No las commitees al repo.

---

## 3. Crear los 5 schemas y aplicar los DDLs

Supabase trae `pgcrypto` ya habilitado, así que el `CREATE EXTENSION IF NOT EXISTS pgcrypto` de nuestros DDLs es no-op. No hace falta hacer nada extra.

En el dashboard de Supabase:

1. Sidebar izquierdo → **SQL Editor**.
2. **+ New query** (o el botón de "New SQL snippet").
3. Pega y ejecuta los 5 DDLs **uno por uno**, en este orden (el orden de los DDLs no importa porque no hay FKs entre schemas, pero por orden mental seguimos el de dependencias del seed):

   1. `db/microservices/localizaciones/01_ddl.sql`
   2. `db/microservices/catalogo/01_ddl.sql`
   3. `db/microservices/clientes/01_ddl.sql`
   4. `db/microservices/seguridad/01_ddl.sql`
   5. `db/microservices/reservas/01_ddl.sql`

   Para cada uno:
   - Abre el archivo en tu editor.
   - Copia todo el contenido.
   - Pégalo en el SQL Editor de Supabase.
   - Click en **Run** (o `Ctrl/Cmd + Enter`).
   - Espera el mensaje **Success. No rows returned**.

> Tip: guarda cada query como un "snippet" en Supabase (botón de
> guardar arriba a la derecha) para reusarlo si lo necesitas.

### Verificación rápida

En el SQL Editor:

```sql
SELECT schema_name
FROM information_schema.schemata
WHERE schema_name IN ('security','audit','catalogo','localizaciones','clientes','reservas')
ORDER BY schema_name;
```

Deberías ver los 6 schemas listados.

```sql
SELECT table_schema, COUNT(*) AS tablas
FROM information_schema.tables
WHERE table_schema IN ('security','audit','catalogo','localizaciones','clientes','reservas')
GROUP BY table_schema
ORDER BY table_schema;
```

Conteo esperado de tablas:
| schema | tablas |
|---|---|
| audit | 2 |
| catalogo | 5 |
| clientes | 2 |
| localizaciones | 4 |
| reservas | 7 |
| security | 7 |

---

## 4. Ejecutar los seeds

Mismo procedimiento que los DDLs, pero **el orden sí importa** porque los seeds de Reservas usan GUIDs estables sembrados por los otros servicios.

Ejecuta en este orden:

1. `db/microservices/localizaciones/02_seed.sql`
2. `db/microservices/catalogo/02_seed.sql`
3. `db/microservices/clientes/02_seed.sql`
4. `db/microservices/seguridad/02_seed.sql`
5. `db/microservices/reservas/02_seed.sql`

### Verificación

```sql
SELECT 'paises'        AS tabla, COUNT(*) FROM localizaciones.paises        UNION ALL
SELECT 'ciudades'      , COUNT(*) FROM localizaciones.ciudades              UNION ALL
SELECT 'localizaciones', COUNT(*) FROM localizaciones.localizaciones        UNION ALL
SELECT 'STOCK'         , COUNT(*) FROM localizaciones.localizacion_extra_stock UNION ALL
SELECT 'MARCAS'        , COUNT(*) FROM catalogo.marca_vehiculos             UNION ALL
SELECT 'CATEGORIAS'    , COUNT(*) FROM catalogo.categoria_vehiculos         UNION ALL
SELECT 'extras'        , COUNT(*) FROM catalogo.extras                      UNION ALL
SELECT 'vehiculos'     , COUNT(*) FROM catalogo.vehiculos                   UNION ALL
SELECT 'mantenimientos', COUNT(*) FROM catalogo.mantenimientos              UNION ALL
SELECT 'clientes'      , COUNT(*) FROM clientes.clientes                    UNION ALL
SELECT 'conductores'   , COUNT(*) FROM clientes.conductores                 UNION ALL
SELECT 'roles'         , COUNT(*) FROM security.roles                       UNION ALL
SELECT 'permisos'      , COUNT(*) FROM security.permisos                    UNION ALL
SELECT 'usuarios_app'  , COUNT(*) FROM security.usuarios_app                UNION ALL
SELECT 'reservas'      , COUNT(*) FROM reservas.reservas                    UNION ALL
SELECT 'contratos'     , COUNT(*) FROM reservas.contratos                   UNION ALL
SELECT 'pagos'         , COUNT(*) FROM reservas.pagos                       UNION ALL
SELECT 'facturas'      , COUNT(*) FROM reservas.facturas;
```

Conteos esperados: 2, 4, 3, 3, 4, 3, 4, 6, 1, 3, 4, 5, 10, 4, 1, 1, 1, 1.

---

## 5. Crear los 5 roles DB con grants por schema

Esto es lo que da el aislamiento. Después de este paso, cada role solo podrá ver SU schema. Los demás le darán "permission denied".

1. Abre `db/microservices/99_supabase_grants.sql` en tu editor local.
2. Busca los **5 placeholders** y **reemplázalos** con las contraseñas del paso 2:
   - `__CHANGE_ME_SEGURIDAD__` → tu `<PASSWORD_SEGURIDAD>`
   - `__CHANGE_ME_CATALOGO__` → tu `<PASSWORD_CATALOGO>`
   - `__CHANGE_ME_localizaciones__` → tu `<PASSWORD_localizaciones>`
   - `__CHANGE_ME_clientes__` → tu `<PASSWORD_clientes>`
   - `__CHANGE_ME_reservas__` → tu `<PASSWORD_reservas>`
3. Pega todo en el SQL Editor de Supabase.
4. **Run**.
5. **Importante**: una vez ejecutado, **revierte los cambios locales** (`git checkout db/microservices/99_supabase_grants.sql`) para que las contraseñas reales no queden en tu working tree. El archivo en el repo se queda con placeholders.

### Verificación

```sql
SELECT rolname
FROM pg_roles
WHERE rolname LIKE 'ms_%'
ORDER BY rolname;
```

Debes ver 5 filas: `ms_catalogo`, `ms_clientes`, `ms_localizaciones`, `ms_reservas`, `ms_seguridad`.

```sql
SELECT grantee, table_schema, COUNT(*) AS tablas
FROM information_schema.role_table_grants
WHERE grantee LIKE 'ms_%' AND privilege_type = 'SELECT'
GROUP BY grantee, table_schema
ORDER BY grantee, table_schema;
```

Cada role solo debe aparecer con su schema. `ms_seguridad` aparece con `security` y `audit` (los dos suyos).

### Prueba de aislamiento

En el SQL Editor, click en el botón de **role switcher** (arriba a la derecha del editor) → **Run as → ms_catalogo** (puede que tengas que abrir una sesión separada o usar un cliente externo, según versión del dashboard).

```sql
SELECT * FROM clientes.clientes LIMIT 1;
```

Debe responder:
```
ERROR: permission denied for schema clientes
```

Si responde con datos, los grants no se aplicaron correctamente. Revisa el paso 5.

---

## 6. Obtener las connection strings y guardarlas en `.env`

1. En tu repo, copia `db/microservices/.env.example` a `db/microservices/.env`:

   ```powershell
   Copy-Item db\microservices\.env.example db\microservices\.env
   ```

2. Abre `db/microservices/.env` y reemplaza:
   - `<PROJECT_REF>` (5 veces) → tu Project Ref del paso 1.
   - `<REGION>` (5 veces) → tu region del paso 1 (formato `us-east-1`, `sa-east-1`, etc.).
   - `<PASSWORD_*>` → cada una con la contraseña correspondiente del paso 2.

3. Verifica que `db/microservices/.env` esté ignorado por git:

   ```powershell
   git check-ignore db\microservices\.env
   ```

   Debe imprimir la ruta. Si no, agrégalo al `.gitignore`:

   ```
   db/microservices/.env
   ```

### Probar una conexión

Desde cualquier cliente PostgreSQL (DBeaver, TablePlus, `psql`...):

```
Host:     aws-0-<REGION>.pooler.supabase.com
Port:     6543
Database: postgres
User:     ms_catalogo.<PROJECT_REF>
Password: <PASSWORD_CATALOGO>
SSL:      require
```

Una vez conectado:
```sql
SELECT COUNT(*) FROM catalogo.vehiculos;     -- OK
SELECT COUNT(*) FROM clientes.clientes;      -- debe fallar con permission denied
```

---

## 7. ¿Pooler o direct connection?

Supabase ofrece 3 modos de conexión:

| Modo | Host | Puerto | Cuándo usar |
|---|---|---|---|
| **Direct** | `db.<PROJECT_REF>.supabase.co` | 5432 | Migraciones / scripts admin / LISTEN-NOTIFY |
| **Session pooler** | `aws-0-<REGION>.pooler.supabase.com` | 5432 | App con transacciones largas o features que requieren conexión "pegajosa" |
| **Transaction pooler** | `aws-0-<REGION>.pooler.supabase.com` | 6543 | API stateless (lo nuestro). **Default recomendado**. |

El `.env.example` ya viene configurado para **transaction pooler** porque es lo correcto para microservicios stateless con .NET / Npgsql. Si más adelante usas EF Core migrations en runtime (no recomendado), considera usar el session pooler para esas operaciones puntuales.

---

## 8. Cuándo migrar a 5 proyectos Supabase reales

Síntomas que indican que ya pediste demasiado a la Opción B:
- La BD se acerca al límite (500 MB en Free, 8 GB en Pro).
- Un servicio "pesado" (ej. reservas en picos) está degradando la latencia del resto.
- Necesitas escalar verticalmente un microservicio sin pagarlo todo.
- Compliance / dominio crítico exige aislamiento físico (auditoría externa).

Cuando llegue ese momento, la migración es directa porque ya tienes los 5 DDL/seed separados:
1. Crea 4 proyectos Supabase adicionales.
2. Para cada uno, ejecuta su DDL + seed (no necesitas el `99_supabase_grants.sql`, ahí el role `postgres` por proyecto ya da el aislamiento).
3. Actualiza el `.env` con las nuevas 5 connection strings (cada una apuntando a un host distinto).
4. No tocas código de aplicación.

---

## 9. Resumen de archivos en este proyecto

| Archivo | Propósito |
|---|---|
| `README.md` | Plan general de la migración a microservicios |
| `SUPABASE.md` | **Este archivo** - guía de despliegue en Supabase |
| `.env.example` | Plantilla de las 5 connection strings |
| `<servicio>/01_ddl.sql` | DDL del schema de cada microservicio (5 archivos) |
| `<servicio>/02_seed.sql` | Seed con datos de prueba (5 archivos) |
| `99_supabase_grants.sql` | Roles DB + GRANTs por schema (post-DDL/seed) |

Orden total de ejecución en Supabase SQL Editor:

```
1.  localizaciones/01_ddl.sql
2.  catalogo/01_ddl.sql
3.  clientes/01_ddl.sql
4.  seguridad/01_ddl.sql
5.  reservas/01_ddl.sql
6.  localizaciones/02_seed.sql
7.  catalogo/02_seed.sql
8.  clientes/02_seed.sql
9.  seguridad/02_seed.sql
10. reservas/02_seed.sql
11. 99_supabase_grants.sql   (con las 5 contraseñas ya reemplazadas)
```

Una vez completado: 5 connection strings listas en `db/microservices/.env`,
una por microservicio, cada una con aislamiento real a nivel de PostgreSQL.
