-- =====================================================
-- MS.Localizaciones - SEED
-- =====================================================
-- Datos semilla con IDs y GUIDs estables para integracion
-- cross-service. Ver db/microservices/README.md seccion 5.
-- =====================================================

-- =====================================================
-- paises
-- =====================================================
INSERT INTO localizaciones.paises (
    id_pais, pais_guid, codigo_iso2, nombre_pais, estado_pais, es_eliminado,
    creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'a0a00000-0000-0000-0000-000000000001', 'EC', 'Ecuador', 'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, 'a0a00000-0000-0000-0000-000000000002', 'PE', 'Perú',    'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_iso2) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('localizaciones.paises','id_pais'),
    (SELECT COALESCE(MAX(id_pais), 1) FROM localizaciones.paises)
);

-- =====================================================
-- ciudades
-- =====================================================
INSERT INTO localizaciones.ciudades (
    id_ciudad, ciudad_guid, id_pais, nombre_ciudad, estado_ciudad, es_eliminado,
    creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'b0b00000-0000-0000-0000-000000000001', 1, 'Quito',     'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, 'b0b00000-0000-0000-0000-000000000002', 1, 'Guayaquil', 'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, 'b0b00000-0000-0000-0000-000000000003', 1, 'Cuenca',    'ACT', FALSE, 'seed_admin', 'SEED'),
    (4, 'b0b00000-0000-0000-0000-000000000004', 2, 'Lima',      'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (id_pais, nombre_ciudad) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('localizaciones.ciudades','id_ciudad'),
    (SELECT COALESCE(MAX(id_ciudad), 1) FROM localizaciones.ciudades)
);

-- =====================================================
-- localizaciones (oficinas)
-- =====================================================
INSERT INTO localizaciones.localizaciones (
    id_localizacion, localizacion_guid, codigo_localizacion, nombre_localizacion, id_ciudad,
    direccion_localizacion, telefono_contacto, correo_contacto,
    horario_atencion, zona_horaria, latitud, longitud,
    estado_localizacion, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, '10c00000-0000-0000-0000-000000000001',
     'LOC-UIO-AEP', 'Aeropuerto Quito', 1,
     'Tababela, Quito', '02-395-1000', 'uio.aeropuerto@redcar.local',
     'Lunes a Domingo 06:00 - 22:00', 'America/Guayaquil', -0.129167, -78.357500,
     'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, '10c00000-0000-0000-0000-000000000002',
     'LOC-UIO-CEN', 'Quito Centro', 1,
     'Av. Amazonas y Colón, Quito', '02-222-3344', 'uio.centro@redcar.local',
     'Lunes a Sábado 08:00 - 18:00', 'America/Guayaquil', -0.180653, -78.467834,
     'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, '10c00000-0000-0000-0000-000000000003',
     'LOC-GYE-AEP', 'Aeropuerto Guayaquil', 2,
     'Av. de las Américas, Guayaquil', '04-216-9000', 'gye.aeropuerto@redcar.local',
     'Lunes a Domingo 06:00 - 23:00', 'America/Guayaquil', -2.157419, -79.883598,
     'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_localizacion) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('localizaciones.localizaciones','id_localizacion'),
    (SELECT COALESCE(MAX(id_localizacion), 1) FROM localizaciones.localizaciones)
);

-- =====================================================
-- localizacion_extra_stock
-- =====================================================
-- Stock de extras fisicos (SILLA-BEBE) por localizacion.
-- id_extra y extra_guid son referencias blandas a catalogo.extras.
INSERT INTO localizaciones.localizacion_extra_stock (
    id_localizacion, id_extra, extra_guid,
    stock_disponible, stock_reservado,
    estado_stock, es_eliminado, creado_por_usuario, origen_registro
)
VALUES
    (1, 2, 'e8a40000-0000-0000-0000-000000000002', 8, 0, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, 2, 'e8a40000-0000-0000-0000-000000000002', 4, 0, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, 2, 'e8a40000-0000-0000-0000-000000000002', 6, 0, 'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (id_localizacion, id_extra) DO NOTHING;
