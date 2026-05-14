-- =====================================================
-- MS.Seguridad - SEED
-- =====================================================
-- Roles, permisos, mapeo roles<->permisos, usuarios demo
-- y un cliente API tecnico (Booking sandbox).
--
-- Cross-service: usuarios_app.id_cliente y cliente_guid son
-- referencias "blandas" a clientes.clientes. Para que el usuario
-- 'cliente.carlos' apunte a CLI-0001, usamos los valores estables
-- definidos en clientes/02_seed.sql.
-- =====================================================

-- =====================================================
-- roles
-- =====================================================
INSERT INTO security.roles (
    id_rol, rol_guid, nombre_rol, descripcion_rol, es_sistema, estado_rol, creado_por_usuario
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, '50105001-0000-0000-0000-000000000001', 'ADMIN',       'Administrador general del sistema',           TRUE, 'ACT', 'seed_admin'),
    (2, '50105001-0000-0000-0000-000000000002', 'AGENTE',      'Usuario operativo de oficina/POS',            TRUE, 'ACT', 'seed_admin'),
    (3, '50105001-0000-0000-0000-000000000003', 'SUPERVISOR',  'Supervisor operativo y de reportes',          TRUE, 'ACT', 'seed_admin'),
    (4, '50105001-0000-0000-0000-000000000004', 'API_PARTNER', 'Cliente técnico para integraciones externas', TRUE, 'ACT', 'seed_admin'),
    (5, '50105001-0000-0000-0000-000000000005', 'CLIENTE_WEB', 'Cliente final del e-commerce',                TRUE, 'ACT', 'seed_admin')
ON CONFLICT (nombre_rol) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('security.roles','id_rol'),
    (SELECT COALESCE(MAX(id_rol), 1) FROM security.roles)
);

-- =====================================================
-- permisos
-- =====================================================
INSERT INTO security.permisos (
    id_permiso, permiso_guid, codigo_permiso, modulo, accion, descripcion_permiso, estado_permiso, creado_por_usuario
)
OVERRIDING SYSTEM VALUE
VALUES
    (1,  '7e7e1000-0000-0000-0000-000000000001', 'VEHICULOS_READ',   'VEHICULOS', 'READ',   'Consultar vehículos',   'ACT', 'seed_admin'),
    (2,  '7e7e1000-0000-0000-0000-000000000002', 'VEHICULOS_UPDATE', 'VEHICULOS', 'UPDATE', 'Actualizar vehículos',  'ACT', 'seed_admin'),
    (3,  '7e7e1000-0000-0000-0000-000000000003', 'RESERVAS_CREATE',  'RESERVAS',  'CREATE', 'Crear reservas',        'ACT', 'seed_admin'),
    (4,  '7e7e1000-0000-0000-0000-000000000004', 'RESERVAS_READ',    'RESERVAS',  'READ',   'Consultar reservas',    'ACT', 'seed_admin'),
    (5,  '7e7e1000-0000-0000-0000-000000000005', 'RESERVAS_UPDATE',  'RESERVAS',  'UPDATE', 'Actualizar reservas',   'ACT', 'seed_admin'),
    (6,  '7e7e1000-0000-0000-0000-000000000006', 'CONTRATOS_CREATE', 'CONTRATOS', 'CREATE', 'Generar contratos',     'ACT', 'seed_admin'),
    (7,  '7e7e1000-0000-0000-0000-000000000007', 'PAGOS_CREATE',     'PAGOS',     'CREATE', 'Registrar pagos',       'ACT', 'seed_admin'),
    (8,  '7e7e1000-0000-0000-0000-000000000008', 'FACTURAS_CREATE',  'FACTURAS',  'CREATE', 'Emitir facturas',       'ACT', 'seed_admin'),
    (9,  '7e7e1000-0000-0000-0000-000000000009', 'REPORTES_READ',    'REPORTES',  'READ',   'Consultar reportes',    'ACT', 'seed_admin'),
    (10, '7e7e1000-0000-0000-0000-00000000000a', 'SEGURIDAD_LOGIN',  'SEGURIDAD', 'LOGIN',  'Iniciar sesión',        'ACT', 'seed_admin')
ON CONFLICT (codigo_permiso) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('security.permisos','id_permiso'),
    (SELECT COALESCE(MAX(id_permiso), 1) FROM security.permisos)
);

-- =====================================================
-- roles_permisos
-- =====================================================
INSERT INTO security.roles_permisos (id_rol, id_permiso, estado_rol_permiso, creado_por_usuario)
SELECT r.id_rol, p.id_permiso, 'ACT', 'seed_admin'
FROM security.roles r
JOIN security.permisos p ON (
       (r.nombre_rol = 'ADMIN')
    OR (r.nombre_rol = 'AGENTE' AND p.codigo_permiso IN (
            'VEHICULOS_READ','RESERVAS_CREATE','RESERVAS_READ','RESERVAS_UPDATE',
            'CONTRATOS_CREATE','PAGOS_CREATE','FACTURAS_CREATE','SEGURIDAD_LOGIN'
       ))
    OR (r.nombre_rol = 'SUPERVISOR' AND p.codigo_permiso IN (
            'VEHICULOS_READ','RESERVAS_READ','REPORTES_READ','SEGURIDAD_LOGIN'
       ))
    OR (r.nombre_rol = 'API_PARTNER' AND p.codigo_permiso IN (
            'VEHICULOS_READ','RESERVAS_CREATE','RESERVAS_READ','SEGURIDAD_LOGIN'
       ))
    OR (r.nombre_rol = 'CLIENTE_WEB' AND p.codigo_permiso IN (
            'VEHICULOS_READ','RESERVAS_READ','RESERVAS_CREATE','SEGURIDAD_LOGIN'
       ))
)
ON CONFLICT (id_rol, id_permiso) DO NOTHING;

-- =====================================================
-- usuarios_app (contraseñas hasheadas con bcrypt - texto plano '12345')
-- =====================================================
INSERT INTO security.usuarios_app (
    username, correo, password_hash, password_salt, password_hint,
    requiere_cambio_password, estado_usuario, es_eliminado, activo,
    intentos_fallidos, id_cliente, cliente_guid,
    fecha_registro_utc, creado_por_usuario
)
SELECT 'admin', 'admin@redcar.local', crypt('12345', s.salt), s.salt, 'ambiente_academico',
       FALSE, 'ACT', FALSE, TRUE, 0, NULL::INT, NULL::UUID,
       CURRENT_TIMESTAMP(0), 'seed_admin'
FROM (SELECT gen_salt('bf') AS salt) s
WHERE NOT EXISTS (SELECT 1 FROM security.usuarios_app WHERE username = 'admin');

INSERT INTO security.usuarios_app (
    username, correo, password_hash, password_salt, password_hint,
    requiere_cambio_password, estado_usuario, es_eliminado, activo,
    intentos_fallidos, id_cliente, cliente_guid,
    fecha_registro_utc, creado_por_usuario
)
SELECT 'agente.uio', 'agente.uio@redcar.local', crypt('12345', s.salt), s.salt, 'ambiente_academico',
       FALSE, 'ACT', FALSE, TRUE, 0, NULL::INT, NULL::UUID,
       CURRENT_TIMESTAMP(0), 'seed_admin'
FROM (SELECT gen_salt('bf') AS salt) s
WHERE NOT EXISTS (SELECT 1 FROM security.usuarios_app WHERE username = 'agente.uio');

INSERT INTO security.usuarios_app (
    username, correo, password_hash, password_salt, password_hint,
    requiere_cambio_password, estado_usuario, es_eliminado, activo,
    intentos_fallidos, id_cliente, cliente_guid,
    fecha_registro_utc, creado_por_usuario
)
SELECT 'supervisor', 'supervisor@redcar.local', crypt('12345', s.salt), s.salt, 'ambiente_academico',
       FALSE, 'ACT', FALSE, TRUE, 0, NULL::INT, NULL::UUID,
       CURRENT_TIMESTAMP(0), 'seed_admin'
FROM (SELECT gen_salt('bf') AS salt) s
WHERE NOT EXISTS (SELECT 1 FROM security.usuarios_app WHERE username = 'supervisor');

-- usuario ligado a un cliente final (CLI-0001 / cliente_guid estable)
INSERT INTO security.usuarios_app (
    username, correo, password_hash, password_salt, password_hint,
    requiere_cambio_password, estado_usuario, es_eliminado, activo,
    intentos_fallidos, id_cliente, cliente_guid,
    fecha_registro_utc, creado_por_usuario
)
SELECT 'cliente.carlos', 'carlos.medina@demo.local', crypt('12345', s.salt), s.salt, 'ambiente_academico',
       FALSE, 'ACT', FALSE, TRUE, 0,
       1, 'c1111111-0000-0000-0000-000000000001'::UUID,
       CURRENT_TIMESTAMP(0), 'seed_admin'
FROM (SELECT gen_salt('bf') AS salt) s
WHERE NOT EXISTS (SELECT 1 FROM security.usuarios_app WHERE username = 'cliente.carlos');

-- =====================================================
-- usuarios_roles
-- =====================================================
INSERT INTO security.usuarios_roles (
    id_usuario, id_rol, estado_usuario_rol, es_eliminado, activo, creado_por_usuario
)
SELECT u.id_usuario, r.id_rol, 'ACT', FALSE, TRUE, 'seed_admin'
FROM security.usuarios_app u
JOIN security.roles r ON (
       (u.username = 'admin'          AND r.nombre_rol = 'ADMIN')
    OR (u.username = 'agente.uio'     AND r.nombre_rol = 'AGENTE')
    OR (u.username = 'supervisor'     AND r.nombre_rol = 'SUPERVISOR')
    OR (u.username = 'cliente.carlos' AND r.nombre_rol = 'CLIENTE_WEB')
)
ON CONFLICT (id_usuario, id_rol) DO NOTHING;

-- =====================================================
-- api_clientes (Booking sandbox)
-- =====================================================
INSERT INTO security.api_clientes (
    codigo_cliente_api, nombre_cliente_api, tipo_autenticacion,
    api_key_hash, client_secret_hash, permite_consulta_disponibilidad,
    permite_sincronizacion_reservas, estado_api_cliente,
    fecha_rotacion_credenciales_utc, creado_por_usuario
)
SELECT
    'BOOKING-SANDBOX', 'Booking Sandbox', 'JWT',
    encode(digest('booking-demo-key',    'sha256'), 'hex'),
    encode(digest('booking-demo-secret', 'sha256'), 'hex'),
    TRUE, TRUE, 'ACT', CURRENT_TIMESTAMP(0), 'seed_admin'
WHERE NOT EXISTS (
    SELECT 1 FROM security.api_clientes WHERE codigo_cliente_api = 'BOOKING-SANDBOX'
);
