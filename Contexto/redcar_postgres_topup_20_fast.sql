-- ==========================================================
-- RedCar / Europcar - Top-up FAST (set-based) hasta 20
-- Sin loops WHILE / pensado para SQL Editor de Supabase.
-- Idempotente e incremental.
-- ==========================================================

SELECT set_config('app.current_user', 'seed_topup_fast', true);
SELECT set_config('app.current_ip', '127.0.0.1', true);
SELECT set_config('app.current_origin', 'DB', true);

-- Opcional: evita timeouts de statement en la sesion actual
SET statement_timeout = '0';

-- ==========================================================
-- CATALOGOS BASE
-- ==========================================================

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.paises
    WHERE es_eliminado = FALSE
),
to_insert AS (
    SELECT
        gs AS idx,
        CHR(65 + ((gs - 1) % 26)) || CHR(65 + ((gs + 7) % 26)) AS codigo_iso2,
        'Pais Topup Fast ' || LPAD(gs::TEXT, 2, '0') AS nombre_pais
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.paises (
    codigo_iso2, nombre_pais, estado_pais, es_eliminado, creado_por_usuario, origen_registro
)
SELECT codigo_iso2, nombre_pais, 'ACT', FALSE, 'seed_topup_fast', 'TOPUP_FAST'
FROM to_insert
ON CONFLICT (codigo_iso2) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.ciudades
    WHERE es_eliminado = FALSE
),
paises AS (
    SELECT id_pais, ROW_NUMBER() OVER (ORDER BY id_pais) AS rn
    FROM rental.paises
    WHERE es_eliminado = FALSE
),
pmeta AS (
    SELECT COUNT(*) AS total FROM paises
),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT p.id_pais
         FROM paises p
         WHERE p.rn = ((gs - 1) % (SELECT total FROM pmeta)) + 1) AS id_pais,
        'Ciudad Topup Fast ' || LPAD(gs::TEXT, 3, '0') AS nombre_ciudad
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.ciudades (
    id_pais, nombre_ciudad, estado_ciudad, es_eliminado, creado_por_usuario, origen_registro
)
SELECT id_pais, nombre_ciudad, 'ACT', FALSE, 'seed_topup_fast', 'TOPUP_FAST'
FROM to_insert
WHERE id_pais IS NOT NULL
ON CONFLICT (id_pais, nombre_ciudad) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.localizaciones
    WHERE es_eliminado = FALSE
),
ciudades AS (
    SELECT id_ciudad, ROW_NUMBER() OVER (ORDER BY id_ciudad) AS rn
    FROM rental.ciudades
    WHERE es_eliminado = FALSE
),
cmeta AS (
    SELECT COUNT(*) AS total FROM ciudades
),
to_insert AS (
    SELECT
        gs AS idx,
        'LOC-TF-' || LPAD(gs::TEXT, 4, '0') AS codigo_localizacion,
        'Localizacion Topup Fast ' || LPAD(gs::TEXT, 3, '0') AS nombre_localizacion,
        (SELECT c.id_ciudad
         FROM ciudades c
         WHERE c.rn = ((gs - 1) % (SELECT total FROM cmeta)) + 1) AS id_ciudad
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.localizaciones (
    codigo_localizacion, nombre_localizacion, id_ciudad,
    direccion_localizacion, telefono_contacto, correo_contacto,
    horario_atencion, zona_horaria, latitud, longitud,
    estado_localizacion, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    codigo_localizacion,
    nombre_localizacion,
    id_ciudad,
    'Direccion Fast ' || idx,
    '09' || LPAD((50000000 + idx)::TEXT, 8, '0'),
    'loc.fast.' || idx || '@redcar.local',
    'Lunes a Domingo 08:00 - 18:00',
    'America/Guayaquil',
    (-2 + (idx % 4))::DECIMAL(9,6),
    (-79 + (idx % 3))::DECIMAL(9,6),
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert
WHERE id_ciudad IS NOT NULL
ON CONFLICT (codigo_localizacion) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.marca_vehiculos
    WHERE es_eliminado = FALSE
)
INSERT INTO rental.marca_vehiculos (
    codigo_marca, nombre_marca, descripcion_marca,
    estado_marca, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    'TFM' || LPAD(gs::TEXT, 2, '0'),
    'Marca Fast ' || LPAD(gs::TEXT, 2, '0'),
    'Marca generada fast',
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_marca) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.categoria_vehiculos
    WHERE es_eliminado = FALSE
)
INSERT INTO rental.categoria_vehiculos (
    codigo_categoria, nombre_categoria, descripcion_categoria,
    kilometraje_ilimitado, limite_km_dia, cargo_km_excedente,
    estado_categoria, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    'TFC' || LPAD(gs::TEXT, 2, '0'),
    'Categoria Fast ' || LPAD(gs::TEXT, 2, '0'),
    'Categoria generada fast',
    (gs % 2 = 0),
    CASE WHEN gs % 2 = 0 THEN NULL ELSE 200 END,
    CASE WHEN gs % 2 = 0 THEN NULL ELSE 0.40 END,
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_categoria) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.extras
    WHERE es_eliminado = FALSE
)
INSERT INTO rental.extras (
    codigo_extra, nombre_extra, descripcion_extra, tipo_extra, requiere_stock,
    valor_fijo, estado_extra, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    'TFE-' || LPAD(gs::TEXT, 4, '0'),
    'Extra Fast ' || LPAD(gs::TEXT, 3, '0'),
    'Extra generado fast',
    CASE gs % 4
        WHEN 0 THEN 'SERVICIO'
        WHEN 1 THEN 'FISICO'
        WHEN 2 THEN 'SEGURO'
        ELSE 'OTRO'
    END,
    (gs % 3 = 0),
    ROUND((3 + (gs % 15))::NUMERIC, 2),
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_extra) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.localizacion_extra_stock
    WHERE es_eliminado = FALSE
),
locs AS (
    SELECT id_localizacion, ROW_NUMBER() OVER (ORDER BY id_localizacion) AS rn
    FROM rental.localizaciones
    WHERE es_eliminado = FALSE
),
exts AS (
    SELECT id_extra, ROW_NUMBER() OVER (ORDER BY id_extra) AS rn
    FROM rental.extras
    WHERE es_eliminado = FALSE AND requiere_stock = TRUE
),
lmeta AS (SELECT COUNT(*) AS total FROM locs),
emeta AS (SELECT COUNT(*) AS total FROM exts),
pairs AS (
    SELECT
        gs AS idx,
        (SELECT l.id_localizacion FROM locs l WHERE l.rn = ((gs - 1) % (SELECT total FROM lmeta)) + 1) AS id_localizacion,
        (SELECT e.id_extra FROM exts e WHERE e.rn = ((gs - 1) % (SELECT total FROM emeta)) + 1) AS id_extra
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.localizacion_extra_stock (
    id_localizacion, id_extra, stock_disponible, stock_reservado,
    estado_stock, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    id_localizacion,
    id_extra,
    5 + (idx % 8),
    0,
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM pairs
WHERE id_localizacion IS NOT NULL
  AND id_extra IS NOT NULL
ON CONFLICT (id_localizacion, id_extra) DO NOTHING;

-- ==========================================================
-- CLIENTES / CONDUCTORES / VEHICULOS
-- ==========================================================

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.clientes
    WHERE es_eliminado = FALSE
)
INSERT INTO rental.clientes (
    codigo_cliente, tipo_identificacion, numero_identificacion,
    cli_nombre1, cli_nombre2, cli_apellido1, cli_apellido2,
    fecha_nacimiento, cli_telefono, cli_correo, direccion_principal,
    estado_cliente, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    'CLF-' || LPAD(gs::TEXT, 4, '0'),
    CASE WHEN gs % 4 = 0 THEN 'PAS' ELSE 'CED' END,
    CASE
        WHEN gs % 4 = 0 THEN 'PF' || LPAD((900000 + gs)::TEXT, 6, '0')
        ELSE LPAD((1900000000 + gs)::TEXT, 10, '0')
    END,
    'NombreFast' || gs,
    NULL,
    'ApellidoFast' || gs,
    NULL,
    DATE '1988-01-01' + (gs * INTERVAL '120 day'),
    '09' || LPAD((60000000 + gs)::TEXT, 8, '0'),
    'cliente.fast.' || gs || '@demo.local',
    'Direccion cliente fast ' || gs,
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_cliente) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.conductores
    WHERE es_eliminado = FALSE
)
INSERT INTO rental.conductores (
    codigo_conductor, tipo_identificacion, numero_identificacion,
    con_nombre1, con_nombre2, con_apellido1, con_apellido2,
    numero_licencia, fecha_vencimiento_licencia, edad_conductor,
    con_telefono, con_correo,
    estado_conductor, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    'COF-' || LPAD(gs::TEXT, 4, '0'),
    CASE WHEN gs % 3 = 0 THEN 'PAS' ELSE 'CED' END,
    CASE
        WHEN gs % 3 = 0 THEN 'PC' || LPAD((700000 + gs)::TEXT, 6, '0')
        ELSE LPAD((1950000000 + gs)::TEXT, 10, '0')
    END,
    'ConFast' || gs,
    NULL,
    'DriverFast' || gs,
    NULL,
    'LIC-FAST-' || LPAD(gs::TEXT, 5, '0'),
    CURRENT_DATE + INTERVAL '700 day',
    22 + (gs % 35),
    '09' || LPAD((70000000 + gs)::TEXT, 8, '0'),
    'conductor.fast.' || gs || '@demo.local',
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_conductor) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.vehiculos
    WHERE es_eliminado = FALSE
),
marcas AS (
    SELECT id_marca, ROW_NUMBER() OVER (ORDER BY id_marca) AS rn
    FROM rental.marca_vehiculos
    WHERE es_eliminado = FALSE
),
cats AS (
    SELECT id_categoria, ROW_NUMBER() OVER (ORDER BY id_categoria) AS rn
    FROM rental.categoria_vehiculos
    WHERE es_eliminado = FALSE
),
locs AS (
    SELECT id_localizacion, ROW_NUMBER() OVER (ORDER BY id_localizacion) AS rn
    FROM rental.localizaciones
    WHERE es_eliminado = FALSE
),
mmeta AS (SELECT COUNT(*) AS total FROM marcas),
cmeta AS (SELECT COUNT(*) AS total FROM cats),
lmeta AS (SELECT COUNT(*) AS total FROM locs),
to_insert AS (
    SELECT
        gs AS idx,
        'VEF-' || LPAD(gs::TEXT, 4, '0') AS codigo_interno_vehiculo,
        'PFT-' || LPAD(gs::TEXT, 4, '0') AS placa_vehiculo,
        (SELECT m.id_marca FROM marcas m WHERE m.rn = ((gs - 1) % (SELECT total FROM mmeta)) + 1) AS id_marca,
        (SELECT c.id_categoria FROM cats c WHERE c.rn = ((gs - 1) % (SELECT total FROM cmeta)) + 1) AS id_categoria,
        (SELECT l.id_localizacion FROM locs l WHERE l.rn = ((gs - 1) % (SELECT total FROM lmeta)) + 1) AS id_localizacion
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.vehiculos (
    codigo_interno_vehiculo, placa_vehiculo, id_marca, id_categoria,
    modelo_vehiculo, anio_fabricacion, color_vehiculo, tipo_combustible,
    tipo_transmision, capacidad_pasajeros, capacidad_maletas, numero_puertas,
    localizacion_actual, precio_base_dia, kilometraje_actual, aire_acondicionado,
    estado_operativo, observaciones_generales, imagen_referencial_url,
    estado_vehiculo, es_eliminado, origen_registro, creado_por_usuario
)
SELECT
    codigo_interno_vehiculo,
    placa_vehiculo,
    id_marca,
    id_categoria,
    'Modelo Fast ' || LPAD(idx::TEXT, 3, '0'),
    (2020 + (idx % 6))::SMALLINT,
    CASE idx % 5 WHEN 0 THEN 'Blanco' WHEN 1 THEN 'Negro' WHEN 2 THEN 'Azul' WHEN 3 THEN 'Gris' ELSE 'Plata' END,
    CASE idx % 3 WHEN 0 THEN 'GASOLINA' WHEN 1 THEN 'HIBRIDO' ELSE 'DIESEL' END,
    CASE WHEN idx % 2 = 0 THEN 'AUTOMATICA' ELSE 'MANUAL' END,
    4 + (idx % 3),
    2 + (idx % 3),
    4,
    id_localizacion,
    ROUND((45 + (idx % 65))::NUMERIC, 2),
    2000 + (idx * 550),
    TRUE,
    'DISPONIBLE',
    'Vehiculo generado fast',
    'images/vehiculos/fast_' || LPAD(idx::TEXT, 3, '0') || '.jpg',
    'ACT',
    FALSE,
    'TOPUP_FAST',
    'seed_topup_fast'
FROM to_insert
WHERE id_marca IS NOT NULL AND id_categoria IS NOT NULL AND id_localizacion IS NOT NULL
ON CONFLICT (codigo_interno_vehiculo) DO NOTHING;

-- ==========================================================
-- SEGURIDAD
-- ==========================================================

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.roles
    WHERE estado_rol = 'ACT'
)
INSERT INTO security.roles (
    nombre_rol, descripcion_rol, es_sistema, estado_rol, creado_por_usuario
)
SELECT
    'FAST_ROLE_' || LPAD(gs::TEXT, 3, '0'),
    'Rol generado fast',
    FALSE,
    'ACT',
    'seed_topup_fast'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (nombre_rol) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.permisos
    WHERE estado_permiso = 'ACT'
)
INSERT INTO security.permisos (
    codigo_permiso, modulo, accion, descripcion_permiso, estado_permiso, creado_por_usuario
)
SELECT
    'FAST_PERM_' || LPAD(gs::TEXT, 3, '0'),
    'FAST',
    'READ',
    'Permiso generado fast',
    'ACT',
    'seed_topup_fast'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_permiso) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.roles_permisos
),
roles AS (
    SELECT id_rol, ROW_NUMBER() OVER (ORDER BY id_rol) AS rn
    FROM security.roles
    WHERE estado_rol = 'ACT'
),
perms AS (
    SELECT id_permiso, ROW_NUMBER() OVER (ORDER BY id_permiso) AS rn
    FROM security.permisos
    WHERE estado_permiso = 'ACT'
),
rmeta AS (SELECT COUNT(*) AS total FROM roles),
pmeta AS (SELECT COUNT(*) AS total FROM perms),
to_insert AS (
    SELECT
        (SELECT r.id_rol FROM roles r WHERE r.rn = ((gs - 1) % (SELECT total FROM rmeta)) + 1) AS id_rol,
        (SELECT p.id_permiso FROM perms p WHERE p.rn = ((gs - 1) % (SELECT total FROM pmeta)) + 1) AS id_permiso
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO security.roles_permisos (
    id_rol, id_permiso, estado_rol_permiso, creado_por_usuario
)
SELECT id_rol, id_permiso, 'ACT', 'seed_topup_fast'
FROM to_insert
WHERE id_rol IS NOT NULL AND id_permiso IS NOT NULL
ON CONFLICT (id_rol, id_permiso) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.usuarios_app
    WHERE es_eliminado = FALSE
),
clientes AS (
    SELECT id_cliente, ROW_NUMBER() OVER (ORDER BY id_cliente) AS rn
    FROM rental.clientes
    WHERE es_eliminado = FALSE
),
cmeta AS (SELECT COUNT(*) AS total FROM clientes)
INSERT INTO security.usuarios_app (
    username, correo, password_hash, password_salt, password_hint,
    requiere_cambio_password, estado_usuario, es_eliminado, activo,
    intentos_fallidos, id_cliente, fecha_registro_utc, creado_por_usuario
)
SELECT
    'fast.user.' || LPAD(gs::TEXT, 3, '0'),
    'fast.user.' || LPAD(gs::TEXT, 3, '0') || '@redcar.local',
    crypt('12345', s.salt),
    s.salt,
    'fast',
    FALSE,
    'ACT',
    FALSE,
    TRUE,
    0,
    (SELECT c.id_cliente FROM clientes c WHERE c.rn = ((gs - 1) % (SELECT total FROM cmeta)) + 1),
    CURRENT_TIMESTAMP(0),
    'seed_topup_fast'
FROM generate_series(1, (SELECT n FROM need)) gs
CROSS JOIN LATERAL (SELECT gen_salt('bf') AS salt) s
ON CONFLICT (username) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.usuarios_roles
    WHERE es_eliminado = FALSE
),
users_ AS (
    SELECT id_usuario, ROW_NUMBER() OVER (ORDER BY id_usuario) AS rn
    FROM security.usuarios_app
    WHERE es_eliminado = FALSE
),
roles AS (
    SELECT id_rol, ROW_NUMBER() OVER (ORDER BY id_rol) AS rn
    FROM security.roles
    WHERE estado_rol = 'ACT'
),
umeta AS (SELECT COUNT(*) AS total FROM users_),
rmeta AS (SELECT COUNT(*) AS total FROM roles),
to_insert AS (
    SELECT
        (SELECT u.id_usuario FROM users_ u WHERE u.rn = ((gs - 1) % (SELECT total FROM umeta)) + 1) AS id_usuario,
        (SELECT r.id_rol FROM roles r WHERE r.rn = ((gs - 1) % (SELECT total FROM rmeta)) + 1) AS id_rol
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO security.usuarios_roles (
    id_usuario, id_rol, estado_usuario_rol, es_eliminado, activo, creado_por_usuario
)
SELECT id_usuario, id_rol, 'ACT', FALSE, TRUE, 'seed_topup_fast'
FROM to_insert
WHERE id_usuario IS NOT NULL AND id_rol IS NOT NULL
ON CONFLICT (id_usuario, id_rol) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM security.api_clientes
    WHERE estado_api_cliente = 'ACT'
)
INSERT INTO security.api_clientes (
    codigo_cliente_api, nombre_cliente_api, tipo_autenticacion,
    api_key_hash, client_secret_hash, permite_consulta_disponibilidad,
    permite_sincronizacion_reservas, estado_api_cliente,
    fecha_rotacion_credenciales_utc, creado_por_usuario
)
SELECT
    'FAST-API-' || LPAD(gs::TEXT, 3, '0'),
    'Fast API Cliente ' || LPAD(gs::TEXT, 3, '0'),
    'JWT',
    encode(digest('fast-key-' || gs, 'sha256'), 'hex'),
    encode(digest('fast-secret-' || gs, 'sha256'), 'hex'),
    TRUE,
    TRUE,
    'ACT',
    CURRENT_TIMESTAMP(0),
    'seed_topup_fast'
FROM generate_series(1, (SELECT n FROM need)) gs
ON CONFLICT (codigo_cliente_api) DO NOTHING;

-- ==========================================================
-- OPERACION
-- ==========================================================

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.reservas
    WHERE es_eliminado = FALSE
),
clientes AS (
    SELECT id_cliente, ROW_NUMBER() OVER (ORDER BY id_cliente) AS rn
    FROM rental.clientes
    WHERE es_eliminado = FALSE AND estado_cliente = 'ACT'
),
vehiculos AS (
    SELECT id_vehiculo, localizacion_actual, ROW_NUMBER() OVER (ORDER BY id_vehiculo) AS rn
    FROM rental.vehiculos
    WHERE es_eliminado = FALSE AND estado_vehiculo = 'ACT'
),
cmeta AS (SELECT COUNT(*) AS total FROM clientes),
vmeta AS (SELECT COUNT(*) AS total FROM vehiculos),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT c.id_cliente FROM clientes c WHERE c.rn = ((gs - 1) % (SELECT total FROM cmeta)) + 1) AS id_cliente,
        (SELECT v.id_vehiculo FROM vehiculos v WHERE v.rn = ((gs - 1) % (SELECT total FROM vmeta)) + 1) AS id_vehiculo,
        (SELECT v.localizacion_actual FROM vehiculos v WHERE v.rn = ((gs - 1) % (SELECT total FROM vmeta)) + 1) AS id_localizacion
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.reservas (
    codigo_reserva, id_cliente, id_vehiculo,
    id_localizacion_recogida, id_localizacion_devolucion,
    canal_reserva, fecha_hora_recogida, fecha_hora_devolucion,
    subtotal, valor_impuestos, valor_extras, valor_deposito_garantia,
    cargo_one_way, total, codigo_confirmacion, estado_reserva,
    requiere_hold, creado_por_usuario, origen_registro
)
SELECT
    'REF-' || LPAD(idx::TEXT, 4, '0'),
    id_cliente,
    id_vehiculo,
    id_localizacion,
    id_localizacion,
    CASE idx % 3 WHEN 0 THEN 'WEB' WHEN 1 THEN 'APP' ELSE 'POS' END,
    CURRENT_TIMESTAMP(0) + (idx * INTERVAL '1 day'),
    CURRENT_TIMESTAMP(0) + (idx * INTERVAL '1 day') + INTERVAL '2 day',
    110 + (idx * 2),
    ROUND((110 + (idx * 2)) * 0.12, 2),
    0,
    150.00,
    0,
    ROUND((110 + (idx * 2)) * 1.12, 2),
    'CFF-' || LPAD(idx::TEXT, 4, '0'),
    'CONFIRMADA',
    TRUE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert
WHERE id_cliente IS NOT NULL AND id_vehiculo IS NOT NULL AND id_localizacion IS NOT NULL
ON CONFLICT (codigo_reserva) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.reserva_conductores
    WHERE es_eliminado = FALSE
),
reservas AS (
    SELECT id_reserva, ROW_NUMBER() OVER (ORDER BY id_reserva) AS rn
    FROM rental.reservas
    WHERE es_eliminado = FALSE
),
conductores AS (
    SELECT id_conductor, ROW_NUMBER() OVER (ORDER BY id_conductor) AS rn
    FROM rental.conductores
    WHERE es_eliminado = FALSE AND estado_conductor = 'ACT'
),
rmeta AS (SELECT COUNT(*) AS total FROM reservas),
cmeta AS (SELECT COUNT(*) AS total FROM conductores),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT r.id_reserva FROM reservas r WHERE r.rn = ((gs - 1) % (SELECT total FROM rmeta)) + 1) AS id_reserva,
        (SELECT c.id_conductor FROM conductores c WHERE c.rn = ((gs - 1) % (SELECT total FROM cmeta)) + 1) AS id_conductor
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.reserva_conductores (
    id_reserva, id_conductor, tipo_conductor, es_principal, cargo_conductor_joven,
    estado_reserva_conductor, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    t.id_reserva,
    t.id_conductor,
    'TITULAR',
    TRUE,
    0,
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert t
WHERE t.id_reserva IS NOT NULL
  AND t.id_conductor IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM rental.reserva_conductores rc
      WHERE rc.id_reserva = t.id_reserva
        AND rc.id_conductor = t.id_conductor
        AND rc.es_eliminado = FALSE
  );

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.reserva_extras
    WHERE es_eliminado = FALSE
),
reservas AS (
    SELECT id_reserva, ROW_NUMBER() OVER (ORDER BY id_reserva) AS rn
    FROM rental.reservas
    WHERE es_eliminado = FALSE
),
extras AS (
    SELECT id_extra, valor_fijo, ROW_NUMBER() OVER (ORDER BY id_extra) AS rn
    FROM rental.extras
    WHERE es_eliminado = FALSE AND estado_extra = 'ACT'
),
rmeta AS (SELECT COUNT(*) AS total FROM reservas),
emeta AS (SELECT COUNT(*) AS total FROM extras),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT r.id_reserva FROM reservas r WHERE r.rn = ((gs - 1) % (SELECT total FROM rmeta)) + 1) AS id_reserva,
        (SELECT e.id_extra FROM extras e WHERE e.rn = ((gs - 1) % (SELECT total FROM emeta)) + 1) AS id_extra,
        (SELECT e.valor_fijo FROM extras e WHERE e.rn = ((gs - 1) % (SELECT total FROM emeta)) + 1) AS valor_fijo
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.reserva_extras (
    id_reserva, id_extra, cantidad, valor_unitario_extra, subtotal_extra,
    estado_reserva_extra, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    t.id_reserva,
    t.id_extra,
    1,
    t.valor_fijo,
    t.valor_fijo,
    'ACT',
    FALSE,
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert t
WHERE t.id_reserva IS NOT NULL
  AND t.id_extra IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM rental.reserva_extras re
      WHERE re.id_reserva = t.id_reserva
        AND re.id_extra = t.id_extra
        AND re.es_eliminado = FALSE
  );

-- Contratos: usa la funcion de negocio para reservas confirmadas sin contrato
DO $$
DECLARE
    v_id_reserva INT;
BEGIN
    FOR v_id_reserva IN
        SELECT r.id_reserva
        FROM rental.reservas r
        LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
        WHERE r.es_eliminado = FALSE
          AND r.estado_reserva = 'CONFIRMADA'
          AND c.id_contrato IS NULL
        ORDER BY r.id_reserva
        LIMIT GREATEST(0, 20 - (SELECT COUNT(*) FROM rental.contratos WHERE es_eliminado = FALSE))
    LOOP
        PERFORM rental.fn_generar_contrato(
            v_id_reserva,
            'seed_topup_fast',
            (SELECT COALESCE(v.kilometraje_actual, 0)
             FROM rental.vehiculos v
             JOIN rental.reservas r2 ON r2.id_vehiculo = v.id_vehiculo
             WHERE r2.id_reserva = v_id_reserva
             LIMIT 1),
            100.00,
            'docs/contratos/fast_' || v_id_reserva || '.pdf',
            'Contrato generado por topup fast',
            'TOPUP_FAST'
        );
    END LOOP;
END $$;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.pagos
    WHERE es_eliminado = FALSE
),
reservas_contrato AS (
    SELECT
        r.id_reserva,
        r.id_cliente,
        c.id_contrato,
        ROW_NUMBER() OVER (ORDER BY r.id_reserva) AS rn
    FROM rental.reservas r
    LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
    WHERE r.es_eliminado = FALSE
),
meta AS (SELECT COUNT(*) AS total FROM reservas_contrato),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT rc.id_reserva FROM reservas_contrato rc WHERE rc.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_reserva,
        (SELECT rc.id_cliente FROM reservas_contrato rc WHERE rc.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_cliente,
        (SELECT rc.id_contrato FROM reservas_contrato rc WHERE rc.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_contrato
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.pagos (
    codigo_pago, id_reserva, id_contrato, id_cliente,
    tipo_pago, metodo_pago, estado_pago, referencia_externa,
    monto, moneda, fecha_pago_utc, observaciones_pago,
    creado_por_usuario, origen_registro
)
SELECT
    'PGF-' || LPAD(idx::TEXT, 4, '0'),
    id_reserva,
    id_contrato,
    id_cliente,
    'COBRO',
    CASE idx % 3 WHEN 0 THEN 'TARJETA' WHEN 1 THEN 'TRANSFERENCIA' ELSE 'EFECTIVO' END,
    'APROBADO',
    'TRX-FAST-' || LPAD(idx::TEXT, 6, '0'),
    ROUND((90 + (idx % 140))::NUMERIC, 2),
    'USD',
    CURRENT_TIMESTAMP(0),
    'Pago generado por topup fast',
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert
WHERE id_reserva IS NOT NULL AND id_cliente IS NOT NULL
ON CONFLICT (codigo_pago) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.facturas
    WHERE es_eliminado = FALSE
),
base AS (
    SELECT
        r.id_reserva,
        r.id_cliente,
        c.id_contrato,
        COALESCE(r.subtotal, 100)::NUMERIC(12,2) AS subtotal,
        ROW_NUMBER() OVER (ORDER BY r.id_reserva) AS rn
    FROM rental.reservas r
    LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
    WHERE r.es_eliminado = FALSE
),
meta AS (SELECT COUNT(*) AS total FROM base),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT b.id_reserva FROM base b WHERE b.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_reserva,
        (SELECT b.id_cliente FROM base b WHERE b.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_cliente,
        (SELECT b.id_contrato FROM base b WHERE b.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_contrato,
        (SELECT b.subtotal FROM base b WHERE b.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS subtotal
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.facturas (
    numero_factura, id_cliente, id_reserva, id_contrato,
    fecha_emision, subtotal, valor_iva, total,
    observaciones_factura, origen_canal_factura, estado_factura,
    es_eliminado, fecha_registro_utc, creado_por_usuario, servicio_origen
)
SELECT
    'FCF-' || LPAD(idx::TEXT, 4, '0'),
    id_cliente,
    id_reserva,
    id_contrato,
    CURRENT_TIMESTAMP(0),
    subtotal,
    ROUND(subtotal * 0.12, 2),
    ROUND(subtotal * 1.12, 2),
    'Factura generada por topup fast',
    'WEB',
    'EMITIDA',
    FALSE,
    CURRENT_TIMESTAMP(0),
    'seed_topup_fast',
    'TOPUP_FAST'
FROM to_insert
WHERE id_reserva IS NOT NULL AND id_cliente IS NOT NULL
ON CONFLICT (numero_factura) DO NOTHING;

WITH need AS (
    SELECT GREATEST(0, 20 - COUNT(*)) AS n
    FROM rental.mantenimientos
    WHERE es_eliminado = FALSE
),
vehiculos AS (
    SELECT id_vehiculo, ROW_NUMBER() OVER (ORDER BY id_vehiculo) AS rn
    FROM rental.vehiculos
    WHERE es_eliminado = FALSE
),
meta AS (SELECT COUNT(*) AS total FROM vehiculos),
to_insert AS (
    SELECT
        gs AS idx,
        (SELECT v.id_vehiculo FROM vehiculos v WHERE v.rn = ((gs - 1) % (SELECT total FROM meta)) + 1) AS id_vehiculo
    FROM generate_series(1, (SELECT n FROM need)) gs
)
INSERT INTO rental.mantenimientos (
    codigo_mantenimiento, id_vehiculo, tipo_mantenimiento,
    fecha_inicio_utc, fecha_fin_utc, kilometraje_mantenimiento,
    costo_mantenimiento, proveedor_taller, estado_mantenimiento,
    observaciones, creado_por_usuario
)
SELECT
    'MTF-' || LPAD(idx::TEXT, 4, '0'),
    id_vehiculo,
    CASE idx % 2 WHEN 0 THEN 'PREVENTIVO' ELSE 'CORRECTIVO' END,
    CURRENT_TIMESTAMP(0) - (idx * INTERVAL '2 day'),
    NULL,
    7000 + (idx * 500),
    ROUND((80 + (idx % 150))::NUMERIC, 2),
    'Taller Fast ' || ((idx % 5) + 1),
    CASE idx % 3 WHEN 0 THEN 'ABIERTO' ELSE 'CERRADO' END,
    'Mantenimiento generado por topup fast',
    'seed_topup_fast'
FROM to_insert
WHERE id_vehiculo IS NOT NULL
ON CONFLICT (codigo_mantenimiento) DO NOTHING;

-- ==========================================================
-- RESUMEN
-- ==========================================================

SELECT 'rental.paises' AS tabla, COUNT(*) AS total FROM rental.paises WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.ciudades', COUNT(*) FROM rental.ciudades WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.localizaciones', COUNT(*) FROM rental.localizaciones WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.marca_vehiculos', COUNT(*) FROM rental.marca_vehiculos WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.categoria_vehiculos', COUNT(*) FROM rental.categoria_vehiculos WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.extras', COUNT(*) FROM rental.extras WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.localizacion_extra_stock', COUNT(*) FROM rental.localizacion_extra_stock WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.clientes', COUNT(*) FROM rental.clientes WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.conductores', COUNT(*) FROM rental.conductores WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.vehiculos', COUNT(*) FROM rental.vehiculos WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.reservas', COUNT(*) FROM rental.reservas WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.reserva_conductores', COUNT(*) FROM rental.reserva_conductores WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.reserva_extras', COUNT(*) FROM rental.reserva_extras WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.contratos', COUNT(*) FROM rental.contratos WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.pagos', COUNT(*) FROM rental.pagos WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.facturas', COUNT(*) FROM rental.facturas WHERE es_eliminado = FALSE
UNION ALL SELECT 'rental.mantenimientos', COUNT(*) FROM rental.mantenimientos WHERE es_eliminado = FALSE
UNION ALL SELECT 'security.roles', COUNT(*) FROM security.roles WHERE estado_rol = 'ACT'
UNION ALL SELECT 'security.permisos', COUNT(*) FROM security.permisos WHERE estado_permiso = 'ACT'
UNION ALL SELECT 'security.roles_permisos', COUNT(*) FROM security.roles_permisos
UNION ALL SELECT 'security.usuarios_app', COUNT(*) FROM security.usuarios_app WHERE es_eliminado = FALSE
UNION ALL SELECT 'security.usuarios_roles', COUNT(*) FROM security.usuarios_roles WHERE es_eliminado = FALSE
UNION ALL SELECT 'security.api_clientes', COUNT(*) FROM security.api_clientes WHERE estado_api_cliente = 'ACT'
ORDER BY 1;

