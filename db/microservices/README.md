# RedCar V2 - Bases de datos por microservicio

Esta carpeta contiene la migración del monolito `Contexto/redcar_postgres_ddl_v2.sql`
a **5 bases de datos independientes**, una por microservicio, siguiendo el principio
**Database-per-Service** de la arquitectura objetivo (`Arquitectura RedCar V2`).

> **Antes**: una sola base con los esquemas `rental`, `security`, `audit`.
> **Ahora**: 5 bases físicas independientes, cada una con su propio esquema de dominio.

---

## 1. Bases de datos y su contenido

| Microservicio | Base de datos | Esquema(s) | Tablas que posee |
|---|---|---|---|
| `MS.Seguridad` | `redcar_seguridad_db` | `security`, `audit` | `usuarios_app`, `roles`, `permisos`, `usuarios_roles`, `roles_permisos`, `sesiones`, `api_clientes`, `aud_eventos`, `aud_intentos_login` |
| `MS.Catalogo` | `redcar_catalogo_db` | `catalogo` | `marca_vehiculos`, `categoria_vehiculos`, `extras`, `vehiculos`, `mantenimientos` |
| `MS.Localizaciones` | `redcar_localizaciones_db` | `localizaciones` | `paises`, `ciudades`, `localizaciones`, `localizacion_extra_stock` |
| `MS.Clientes` | `redcar_clientes_db` | `clientes` | `clientes`, `conductores` |
| `MS.Reservas + Facturacion` | `redcar_reservas_db` | `reservas` | `reservas`, `reserva_conductores`, `reserva_extras`, `contratos`, `checkin_out`, `pagos`, `facturas` |

---

## 2. Principios de la migración

### 2.0 Nombres de tablas en minúsculas

En PostgreSQL, los identificadores **sin comillas dobles** se normalizan a
minúsculas. Los DDL usan siempre nombres de tabla en **snake_case minúscula**
(`paises`, `vehiculos`, `reserva_conductores`, …) para que coincida con el
catálogo en Supabase y no haya ambigüedad: `"PAISES"` (con comillas) y
`paises` son dos objetos distintos; sin comillas, solo existe `paises`.

### 2.1 Sin FOREIGN KEYs entre bases
Cada microservicio es dueño exclusivo de sus tablas. Las relaciones que en el
monolito eran FK entre esquemas, **ya no se enforcean en la base**: se vuelven
**referencias "blandas" por ID + GUID**, y la integridad la garantiza la capa
de negocio del orquestador (`Middleware.RedCar.Api`).

### 2.2 Doble columna de referencia: `id_*` + `*_guid`
Donde antes había `id_cliente INT NOT NULL REFERENCES rental.clientes`,
ahora hay **dos columnas** sin FK:

```sql
id_cliente    INT  NOT NULL,   -- referencia local (legacy / rendimiento)
cliente_guid  UUID NULL,       -- referencia canónica cross-service
```

- El `id_*` se conserva para no romper código legacy y para reportes locales.
- El `*_guid` es la identidad **canónica** que viaja entre microservicios
  por REST/gRPC. Es la única que un servicio consumidor debe usar para llamar
  al servicio dueño.

### 2.3 FKs que sí se mantienen (mismo dominio)
- `localizaciones.ciudades.id_pais` → `localizaciones.paises`
- `localizaciones.localizaciones.id_ciudad` → `localizaciones.ciudades`
- `catalogo.vehiculos.id_marca` → `catalogo.marca_vehiculos`
- `catalogo.vehiculos.id_categoria` → `catalogo.categoria_vehiculos`
- `catalogo.mantenimientos.id_vehiculo` → `catalogo.vehiculos`
- `clientes.conductores.id_cliente` → `clientes.clientes`
- `reservas.contratos.id_reserva` → `reservas.reservas`
- `reservas.checkin_out.id_contrato` → `reservas.contratos`
- `reservas.pagos.id_reserva|id_contrato` → `reservas.reservas|contratos`
- `reservas.facturas.id_reserva|id_contrato` → `reservas.reservas|contratos`
- `reservas.reserva_conductores.id_reserva` → `reservas.reservas`
- `reservas.reserva_extras.id_reserva` → `reservas.reservas`
- `security.usuarios_roles`, `security.roles_permisos`, `security.sesiones` (todas internas)

### 2.4 FKs cross-service eliminadas
| Columna | Apunta a (otro servicio) |
|---|---|
| `security.usuarios_app.id_cliente` | Clientes |
| `catalogo.vehiculos.localizacion_actual` | Localizaciones |
| `localizaciones.localizacion_extra_stock.id_extra` | Catalogo |
| `reservas.reservas.id_cliente` | Clientes |
| `reservas.reservas.id_vehiculo` | Catalogo |
| `reservas.reservas.id_localizacion_recogida` | Localizaciones |
| `reservas.reservas.id_localizacion_devolucion` | Localizaciones |
| `reservas.reserva_conductores.id_conductor` | Clientes |
| `reservas.reserva_extras.id_extra` | Catalogo |
| `reservas.contratos.id_cliente` | Clientes |
| `reservas.contratos.id_vehiculo` | Catalogo |
| `reservas.pagos.id_cliente` | Clientes |
| `reservas.facturas.id_cliente` | Clientes |

Para todas se añadió el `*_guid` denormalizado correspondiente.

---

## 3. Estructura de archivos

```
db/microservices/
├── README.md                            ← este archivo (plan general)
├── SUPABASE.md                          ← guía paso a paso para subir a Supabase
├── .env.example                         ← plantilla de connection strings (.env real ignorado por git)
├── 99_supabase_grants.sql               ← roles DB + GRANTs por schema (post-DDL/seed, solo Supabase)
├── seguridad/
│   ├── 01_ddl.sql
│   ├── 02_seed.sql
│   └── 03_disable_rls_for_service_roles.sql  ← Supabase: quitar RLS que bloquea roles ms_* (post-grants)
├── catalogo/
│   ├── 01_ddl.sql
│   └── 02_seed.sql
├── localizaciones/
│   ├── 01_ddl.sql
│   └── 02_seed.sql
├── clientes/
│   ├── 01_ddl.sql
│   └── 02_seed.sql
└── reservas/
    ├── 01_ddl.sql
    └── 02_seed.sql
```

> Para desplegar en Supabase (1 proyecto, 5 schemas, 5 roles con aislamiento por GRANTs),
> sigue la guía completa en [`SUPABASE.md`](./SUPABASE.md).

---

## 4. Orden de ejecución (importante)

Los seeds dependen de que los GUIDs cross-service ya existan en el servicio dueño.
Por eso el **orden recomendado** es:

```
1. localizaciones   (no depende de nadie)
2. catalogo         (su seed usa GUIDs de localizaciones para vehiculos)
3. clientes         (no depende de nadie)
4. seguridad        (su seed referencia GUIDs de clientes opcionalmente)
5. reservas         (depende de todos los anteriores)
```

Los DDLs pueden ejecutarse en cualquier orden porque **no hay FKs entre bases**.
Solo los **seeds** tienen un orden lógico recomendado.

### Comandos de ejemplo (PostgreSQL local)

```bash
# 1. Crear cada base
psql -U postgres -c "CREATE DATABASE redcar_seguridad_db;"
psql -U postgres -c "CREATE DATABASE redcar_catalogo_db;"
psql -U postgres -c "CREATE DATABASE redcar_localizaciones_db;"
psql -U postgres -c "CREATE DATABASE redcar_clientes_db;"
psql -U postgres -c "CREATE DATABASE redcar_reservas_db;"

# 2. Ejecutar DDLs
psql -U postgres -d redcar_localizaciones_db  -f db/microservices/localizaciones/01_ddl.sql
psql -U postgres -d redcar_catalogo_db        -f db/microservices/catalogo/01_ddl.sql
psql -U postgres -d redcar_clientes_db        -f db/microservices/clientes/01_ddl.sql
psql -U postgres -d redcar_seguridad_db       -f db/microservices/seguridad/01_ddl.sql
psql -U postgres -d redcar_reservas_db        -f db/microservices/reservas/01_ddl.sql

# 3. Ejecutar seeds (orden importa)
psql -U postgres -d redcar_localizaciones_db  -f db/microservices/localizaciones/02_seed.sql
psql -U postgres -d redcar_catalogo_db        -f db/microservices/catalogo/02_seed.sql
psql -U postgres -d redcar_clientes_db        -f db/microservices/clientes/02_seed.sql
psql -U postgres -d redcar_seguridad_db       -f db/microservices/seguridad/02_seed.sql
psql -U postgres -d redcar_reservas_db        -f db/microservices/reservas/02_seed.sql
```

---

## 5. GUIDs estables para datos semilla cross-service

Para que los seeds funcionen sin tener que “consultar” el servicio dueño,
los datos semilla usan **GUIDs deterministas** acordados (formato fácil de leer):

| Entidad | Código | GUID estable |
|---|---|---|
| Cliente | CLI-0001 | `c1111111-0000-0000-0000-000000000001` |
| Cliente | CLI-0002 | `c1111111-0000-0000-0000-000000000002` |
| Cliente | CLI-0003 | `c1111111-0000-0000-0000-000000000003` |
| Conductor | CON-0001 | `c2222222-0000-0000-0000-000000000001` |
| Conductor | CON-0002 | `c2222222-0000-0000-0000-000000000002` |
| Conductor | CON-0003 | `c2222222-0000-0000-0000-000000000003` |
| Conductor | CON-0004 | `c2222222-0000-0000-0000-000000000004` |
| Localización | LOC-UIO-AEP | `10c00000-0000-0000-0000-000000000001` |
| Localización | LOC-UIO-CEN | `10c00000-0000-0000-0000-000000000002` |
| Localización | LOC-GYE-AEP | `10c00000-0000-0000-0000-000000000003` |
| Vehículo | VEH-0001 | `7e1c0000-0000-0000-0000-000000000001` |
| Vehículo | VEH-0002 | `7e1c0000-0000-0000-0000-000000000002` |
| Vehículo | VEH-0003 | `7e1c0000-0000-0000-0000-000000000003` |
| Vehículo | VEH-0004 | `7e1c0000-0000-0000-0000-000000000004` |
| Vehículo | VEH-0005 | `7e1c0000-0000-0000-0000-000000000005` |
| Vehículo | VEH-0006 | `7e1c0000-0000-0000-0000-000000000006` |
| Extra | GPS | `e8a40000-0000-0000-0000-000000000001` |
| Extra | SILLA-BEBE | `e8a40000-0000-0000-0000-000000000002` |
| Extra | COND-ADIC | `e8a40000-0000-0000-0000-000000000003` |
| Extra | SEGURO-PREM | `e8a40000-0000-0000-0000-000000000004` |

En producción, cada servicio genera sus propios GUIDs (`gen_random_uuid()`)
y el orquestador propaga el GUID al hacer la composición de la operación.

---

## 6. Variables de entorno por servicio

Cada microservicio tendrá su propia connection string. Convención sugerida:

```
ConnectionStrings__SeguridadDb=Host=...;Database=redcar_seguridad_db;...
ConnectionStrings__CatalogoDb=Host=...;Database=redcar_catalogo_db;...
ConnectionStrings__LocalizacionesDb=Host=...;Database=redcar_localizaciones_db;...
ConnectionStrings__ClientesDb=Host=...;Database=redcar_clientes_db;...
ConnectionStrings__ReservasDb=Host=...;Database=redcar_reservas_db;...
```

El orquestador (`Middleware.RedCar.Api`) no consume directamente las bases:
habla por REST/gRPC con cada microservicio.

---

## 7. Migración desde el monolito existente

El DDL monolítico original (`Contexto/redcar_postgres_ddl_v2.sql`) y su seed
**se conservan** como referencia histórica. La migración de datos productivos
del monolito a las 5 bases nuevas se hará con un script de ETL aparte cuando
se decida el corte. Por ahora estas 5 bases reemplazan al monolito en local
y en el ambiente de pruebas.
