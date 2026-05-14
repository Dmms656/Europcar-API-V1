-- =====================================================
-- MS.Clientes - SEED
-- =====================================================
-- Datos semilla con IDs y GUIDs estables.
-- Los GUIDs se usan en otros servicios (Seguridad, Reservas)
-- para referenciar a estos clientes/conductores.
-- =====================================================

-- =====================================================
-- clientes
-- =====================================================
INSERT INTO clientes.clientes (
    id_cliente, cliente_guid, codigo_cliente, tipo_identificacion, numero_identificacion,
    cli_nombre1, cli_nombre2, cli_apellido1, cli_apellido2,
    fecha_nacimiento, cli_telefono, cli_correo, direccion_principal,
    estado_cliente, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'c1111111-0000-0000-0000-000000000001',
     'CLI-0001', 'CED', '1712345678',
     'Carlos', NULL, 'Medina', 'Pérez',
     DATE '1990-05-14', '0991111111', 'carlos.medina@demo.local', 'Quito - La Carolina',
     'ACT', FALSE, 'seed_admin', 'SEED'),

    (2, 'c1111111-0000-0000-0000-000000000002',
     'CLI-0002', 'CED', '0923456789',
     'María', 'José', 'Vera', 'Gómez',
     DATE '1995-08-23', '0982222222', 'maria.vera@demo.local', 'Guayaquil - Urdesa',
     'ACT', FALSE, 'seed_admin', 'SEED'),

    (3, 'c1111111-0000-0000-0000-000000000003',
     'CLI-0003', 'PAS', 'P99887766',
     'John', NULL, 'Miller', NULL,
     DATE '1988-02-11', '0973333333', 'john.miller@demo.local', 'Quito - Cumbayá',
     'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_cliente) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('clientes.clientes','id_cliente'),
    (SELECT COALESCE(MAX(id_cliente), 1) FROM clientes.clientes)
);

-- =====================================================
-- conductores (FK a clientes dentro de la misma BD)
-- =====================================================
INSERT INTO clientes.conductores (
    id_conductor, conductor_guid, codigo_conductor, id_cliente,
    tipo_identificacion, numero_identificacion,
    con_nombre1, con_nombre2, con_apellido1, con_apellido2,
    numero_licencia, fecha_vencimiento_licencia, edad_conductor,
    con_telefono, con_correo,
    estado_conductor, es_eliminado, creado_por_usuario, origen_registro
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, 'c2222222-0000-0000-0000-000000000001',
     'CON-0001', 1, 'CED', '1712345678',
     'Carlos', NULL, 'Medina', 'Pérez',
     'LIC-UIO-0001', DATE '2028-12-31', 35, '0991111111', 'carlos.medina@demo.local',
     'ACT', FALSE, 'seed_admin', 'SEED'),

    (2, 'c2222222-0000-0000-0000-000000000002',
     'CON-0002', 2, 'CED', '0923456789',
     'María', 'José', 'Vera', 'Gómez',
     'LIC-GYE-0002', DATE '2027-10-15', 30, '0982222222', 'maria.vera@demo.local',
     'ACT', FALSE, 'seed_admin', 'SEED'),

    (3, 'c2222222-0000-0000-0000-000000000003',
     'CON-0003', 3, 'PAS', 'P99887766',
     'John', NULL, 'Miller', NULL,
     'LIC-PAS-0003', DATE '2029-06-20', 37, '0973333333', 'john.miller@demo.local',
     'ACT', FALSE, 'seed_admin', 'SEED'),

    (4, 'c2222222-0000-0000-0000-000000000004',
     'CON-0004', NULL, 'CED', '1102233445',
     'Lucía', NULL, 'Andrade', 'Lopez',
     'LIC-UIO-0004', DATE '2028-03-01', 23, '0964444444', 'lucia.andrade@demo.local',
     'ACT', FALSE, 'seed_admin', 'SEED')
ON CONFLICT (codigo_conductor) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('clientes.conductores','id_conductor'),
    (SELECT COALESCE(MAX(id_conductor), 1) FROM clientes.conductores)
);
