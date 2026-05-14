-- =====================================================
-- MS.Catalogo - SEED
-- =====================================================
-- Datos semilla con IDs y GUIDs estables. Las referencias a
-- localizaciones (id_localizacion, localizacion_guid) son
-- "blandas" - el GUID se sembrara con el mismo valor en
-- redcar_localizaciones_db.
-- =====================================================

-- =====================================================
-- marca_vehiculos
-- =====================================================
INSERT INTO catalogo.marca_vehiculos (
    id_marca, marca_guid, codigo_marca, nombre_marca, descripcion_marca,
    estado_marca, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, '6a710000-0000-0000-0000-000000000001', 'TOY', 'Toyota',    'Marca japonesa',        'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, '6a710000-0000-0000-0000-000000000002', 'CHE', 'Chevrolet', 'Marca americana',       'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, '6a710000-0000-0000-0000-000000000003', 'SUZ', 'Suzuki',    'Marca compacta y SUV',  'ACT', FALSE, 'seed_admin', 'SEED'),
    (4, '6a710000-0000-0000-0000-000000000004', 'KIA', 'Kia',       'Marca coreana',         'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_marca) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('catalogo.marca_vehiculos','id_marca'),
    (SELECT COALESCE(MAX(id_marca), 1) FROM catalogo.marca_vehiculos)
);

-- =====================================================
-- categoria_vehiculos
-- =====================================================
INSERT INTO catalogo.categoria_vehiculos (
    id_categoria, categoria_guid, codigo_categoria, nombre_categoria, descripcion_categoria,
    kilometraje_ilimitado, limite_km_dia, cargo_km_excedente,
    estado_categoria, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'ca700000-0000-0000-0000-000000000001', 'ECO', 'Económico', 'Vehículos compactos para ciudad',          TRUE,  NULL, NULL, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, 'ca700000-0000-0000-0000-000000000002', 'SUV', 'SUV',       'Vehículos familiares y todoterreno liviano', TRUE, NULL, NULL, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, 'ca700000-0000-0000-0000-000000000003', 'LUX', 'Luxury',    'Vehículos premium con kilometraje limitado', FALSE, 200,  0.50, 'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_categoria) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('catalogo.categoria_vehiculos','id_categoria'),
    (SELECT COALESCE(MAX(id_categoria), 1) FROM catalogo.categoria_vehiculos)
);

-- =====================================================
-- extras (GUID estable para que otros servicios referencien)
-- =====================================================
INSERT INTO catalogo.extras (
    id_extra, extra_guid, codigo_extra, nombre_extra, descripcion_extra,
    tipo_extra, requiere_stock, valor_fijo,
    estado_extra, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'e8a40000-0000-0000-0000-000000000001', 'GPS',         'GPS',                  'Navegador GPS incluido en el vehículo',          'SERVICIO', FALSE,  5.00, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (2, 'e8a40000-0000-0000-0000-000000000002', 'SILLA-BEBE',  'Silla de bebé',        'Silla de seguridad para niño hasta 18 kg',       'FISICO',   TRUE,   7.50, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (3, 'e8a40000-0000-0000-0000-000000000003', 'COND-ADIC',   'Conductor adicional',  'Autoriza un conductor adicional en la renta',    'SERVICIO', FALSE, 12.00, 'ACT', FALSE, 'seed_admin', 'SEED'),
    (4, 'e8a40000-0000-0000-0000-000000000004', 'SEGURO-PREM', 'Seguro premium',       'Cobertura ampliada de daños y asistencia',       'SEGURO',   FALSE, 18.00, 'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_extra) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('catalogo.extras','id_extra'),
    (SELECT COALESCE(MAX(id_extra), 1) FROM catalogo.extras)
);

-- =====================================================
-- vehiculos (cross-service: localizacion_actual + localizacion_guid)
-- =====================================================
INSERT INTO catalogo.vehiculos (
    id_vehiculo, vehiculo_guid, codigo_interno_vehiculo, placa_vehiculo,
    id_marca, id_categoria,
    modelo_vehiculo, anio_fabricacion, color_vehiculo, tipo_combustible,
    tipo_transmision, capacidad_pasajeros, capacidad_maletas, numero_puertas,
    localizacion_actual, localizacion_guid,
    precio_base_dia, kilometraje_actual, aire_acondicionado,
    estado_operativo, observaciones_generales, imagen_referencial_url,
    estado_vehiculo, es_eliminado, origen_registro, creado_por_usuario
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, '7e1c0000-0000-0000-0000-000000000001',
     'VEH-0001', 'PCD-1001', 3, 2,
     'Grand Vitara', 2022, 'Gris', 'GASOLINA', 'AUTOMATICA', 5, 3, 4,
     1, '10c00000-0000-0000-0000-000000000001',
     85.00, 25500, TRUE,
     'DISPONIBLE', 'SUV principal para aeropuerto Quito', 'images/vehiculos/suzuki_grand_vitara.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin'),

    (2, '7e1c0000-0000-0000-0000-000000000002',
     'VEH-0002', 'PCD-1002', 1, 1,
     'Yaris', 2023, 'Blanco', 'GASOLINA', 'AUTOMATICA', 5, 2, 4,
     2, '10c00000-0000-0000-0000-000000000002',
     48.00, 12600, TRUE,
     'DISPONIBLE', 'Vehículo urbano', 'images/vehiculos/toyota_yaris.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin'),

    (3, '7e1c0000-0000-0000-0000-000000000003',
     'VEH-0003', 'PCD-1003', 2, 1,
     'Onix', 2022, 'Azul', 'GASOLINA', 'MANUAL', 5, 2, 4,
     3, '10c00000-0000-0000-0000-000000000003',
     45.00, 31200, TRUE,
     'DISPONIBLE', 'Económico para alta rotación', 'images/vehiculos/chevrolet_onix.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin'),

    (4, '7e1c0000-0000-0000-0000-000000000004',
     'VEH-0004', 'PCD-1004', 4, 2,
     'Sportage', 2024, 'Negro', 'HIBRIDO', 'AUTOMATICA', 5, 4, 4,
     1, '10c00000-0000-0000-0000-000000000001',
     96.00, 8200, TRUE,
     'DISPONIBLE', 'SUV híbrida', 'images/vehiculos/kia_sportage.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin'),

    (5, '7e1c0000-0000-0000-0000-000000000005',
     'VEH-0005', 'PCD-1005', 1, 3,
     'Camry', 2024, 'Plata', 'HIBRIDO', 'AUTOMATICA', 5, 3, 4,
     1, '10c00000-0000-0000-0000-000000000001',
     120.00, 6800, TRUE,
     'DISPONIBLE', 'Categoría luxury con kilometraje limitado', 'images/vehiculos/toyota_camry.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin'),

    (6, '7e1c0000-0000-0000-0000-000000000006',
     'VEH-0006', 'PCD-1006', 3, 2,
     'Jimny', 2023, 'Verde', 'GASOLINA', 'MANUAL', 4, 2, 3,
     3, '10c00000-0000-0000-0000-000000000003',
     78.00, 14000, TRUE,
     'DISPONIBLE', 'SUV compacta', 'images/vehiculos/suzuki_jimny.jpg',
     'ACT', FALSE, 'SEED', 'seed_admin')
ON CONFLICT (codigo_interno_vehiculo) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('catalogo.vehiculos','id_vehiculo'),
    (SELECT COALESCE(MAX(id_vehiculo), 1) FROM catalogo.vehiculos)
);

-- =====================================================
-- mantenimientos (interno: FK a vehiculos)
-- =====================================================
INSERT INTO catalogo.mantenimientos (
    codigo_mantenimiento, id_vehiculo, tipo_mantenimiento,
    fecha_inicio_utc, fecha_fin_utc, kilometraje_mantenimiento,
    costo_mantenimiento, proveedor_taller, estado_mantenimiento,
    observaciones, creado_por_usuario
)
SELECT
    'MNT-0001', 6, 'PREVENTIVO',
    CURRENT_TIMESTAMP(0) - INTERVAL '1 day', NULL,
    14000, 120.00, 'Taller Norte', 'ABIERTO',
    'Cambio de aceite y revisión general', 'seed_admin'
WHERE NOT EXISTS (
    SELECT 1 FROM catalogo.mantenimientos WHERE codigo_mantenimiento = 'MNT-0001'
);
