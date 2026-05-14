-- =====================================================
-- MS.Reservas + Facturacion - SEED
-- =====================================================
-- Reserva semilla RES-0001 con su contrato, pago y factura.
--
-- TODAS las referencias cross-service usan id + guid estables
-- definidos en clientes/02_seed.sql, catalogo/02_seed.sql,
-- localizaciones/02_seed.sql. Ver db/microservices/README.md
-- seccion 5 para el mapeo completo.
-- =====================================================

-- =====================================================
-- RESERVA RES-0001 (cliente CLI-0001 + vehiculo VEH-0001 + LOC-UIO-AEP)
-- =====================================================
INSERT INTO reservas.reservas (
    codigo_reserva,
    id_cliente, cliente_guid,
    id_vehiculo, vehiculo_guid,
    id_localizacion_recogida, localizacion_recogida_guid,
    id_localizacion_devolucion, localizacion_devolucion_guid,
    canal_reserva, fecha_hora_recogida, fecha_hora_devolucion,
    subtotal, valor_impuestos, valor_extras, valor_deposito_garantia,
    cargo_one_way, total, codigo_confirmacion, estado_reserva,
    requiere_hold, creado_por_usuario, origen_registro
)
SELECT
    'RES-0001',
    1, 'c1111111-0000-0000-0000-000000000001'::UUID,
    1, '7e1c0000-0000-0000-0000-000000000001'::UUID,
    1, '10c00000-0000-0000-0000-000000000001'::UUID,
    1, '10c00000-0000-0000-0000-000000000001'::UUID,
    'WEB',
    CURRENT_TIMESTAMP(0) + INTERVAL '7 day',
    CURRENT_TIMESTAMP(0) + INTERVAL '10 day',
    255.00, 30.60, 12.50, 150.00,
    0, 298.10, 'CNF-0001', 'CONFIRMADA',
    TRUE, 'seed_admin', 'SEED'
WHERE NOT EXISTS (SELECT 1 FROM reservas.reservas WHERE codigo_reserva = 'RES-0001');

-- =====================================================
-- reserva_conductores (CON-0001 como titular)
-- =====================================================
INSERT INTO reservas.reserva_conductores (
    id_reserva, id_conductor, conductor_guid,
    tipo_conductor, es_principal, cargo_conductor_joven,
    estado_reserva_conductor, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    1, 'c2222222-0000-0000-0000-000000000001'::UUID,
    'TITULAR', TRUE, 0,
    'ACT', FALSE, 'seed_admin', 'SEED'
WHERE EXISTS (SELECT 1 FROM reservas.reservas WHERE codigo_reserva = 'RES-0001')
  AND NOT EXISTS (
    SELECT 1 FROM reservas.reserva_conductores rc
    JOIN reservas.reservas r ON r.id_reserva = rc.id_reserva
    WHERE r.codigo_reserva = 'RES-0001' AND rc.id_conductor = 1
);

-- =====================================================
-- reserva_extras (GPS + SILLA-BEBE)
-- =====================================================
INSERT INTO reservas.reserva_extras (
    id_reserva, id_extra, extra_guid,
    cantidad, valor_unitario_extra, subtotal_extra,
    estado_reserva_extra, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    1, 'e8a40000-0000-0000-0000-000000000001'::UUID,
    1, 5.00, 5.00,
    'ACT', FALSE, 'seed_admin', 'SEED'
WHERE EXISTS (SELECT 1 FROM reservas.reservas WHERE codigo_reserva = 'RES-0001')
  AND NOT EXISTS (
    SELECT 1 FROM reservas.reserva_extras re
    JOIN reservas.reservas r ON r.id_reserva = re.id_reserva
    WHERE r.codigo_reserva = 'RES-0001' AND re.id_extra = 1
);

INSERT INTO reservas.reserva_extras (
    id_reserva, id_extra, extra_guid,
    cantidad, valor_unitario_extra, subtotal_extra,
    estado_reserva_extra, es_eliminado, creado_por_usuario, origen_registro
)
SELECT
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    2, 'e8a40000-0000-0000-0000-000000000002'::UUID,
    1, 7.50, 7.50,
    'ACT', FALSE, 'seed_admin', 'SEED'
WHERE EXISTS (SELECT 1 FROM reservas.reservas WHERE codigo_reserva = 'RES-0001')
  AND NOT EXISTS (
    SELECT 1 FROM reservas.reserva_extras re
    JOIN reservas.reservas r ON r.id_reserva = re.id_reserva
    WHERE r.codigo_reserva = 'RES-0001' AND re.id_extra = 2
);

-- =====================================================
-- CONTRATO de la reserva RES-0001
-- =====================================================
INSERT INTO reservas.contratos (
    numero_contrato, id_reserva,
    id_cliente, cliente_guid,
    id_vehiculo, vehiculo_guid,
    fecha_hora_salida, fecha_hora_prevista_devolucion,
    kilometraje_salida, nivel_combustible_salida,
    estado_contrato, pdf_url, observaciones_contrato,
    creado_por_usuario, origen_registro
)
SELECT
    'CTR-RES-0001',
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    1, 'c1111111-0000-0000-0000-000000000001'::UUID,
    1, '7e1c0000-0000-0000-0000-000000000001'::UUID,
    CURRENT_TIMESTAMP(0) + INTERVAL '7 day',
    CURRENT_TIMESTAMP(0) + INTERVAL '10 day',
    25500, 100.00,
    'ABIERTO', 'docs/contratos/CTR-RES-0001.pdf', 'Contrato semilla para pruebas',
    'seed_admin', 'SEED'
WHERE EXISTS (SELECT 1 FROM reservas.reservas WHERE codigo_reserva = 'RES-0001')
  AND NOT EXISTS (
    SELECT 1 FROM reservas.contratos WHERE numero_contrato = 'CTR-RES-0001'
);

-- =====================================================
-- PAGO
-- =====================================================
INSERT INTO reservas.pagos (
    codigo_pago, id_reserva, id_contrato,
    id_cliente, cliente_guid,
    tipo_pago, metodo_pago, estado_pago, referencia_externa,
    monto, moneda, fecha_pago_utc, observaciones_pago,
    creado_por_usuario, origen_registro
)
SELECT
    'PAG-0001',
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    (SELECT id_contrato FROM reservas.contratos WHERE numero_contrato = 'CTR-RES-0001'),
    1, 'c1111111-0000-0000-0000-000000000001'::UUID,
    'COBRO', 'TARJETA', 'APROBADO', 'TRX-DEMO-0001',
    298.10, 'USD', CURRENT_TIMESTAMP(0), 'Cobro inicial de reserva semilla',
    'seed_admin', 'SEED'
WHERE NOT EXISTS (SELECT 1 FROM reservas.pagos WHERE codigo_pago = 'PAG-0001');

-- =====================================================
-- FACTURA
-- =====================================================
INSERT INTO reservas.facturas (
    numero_factura,
    id_cliente, cliente_guid,
    id_reserva, id_contrato,
    fecha_emision, subtotal, valor_iva, total,
    observaciones_factura, origen_canal_factura, estado_factura,
    es_eliminado, fecha_registro_utc, creado_por_usuario, servicio_origen
)
SELECT
    'FAC-0001',
    1, 'c1111111-0000-0000-0000-000000000001'::UUID,
    (SELECT id_reserva FROM reservas.reservas WHERE codigo_reserva = 'RES-0001'),
    (SELECT id_contrato FROM reservas.contratos WHERE numero_contrato = 'CTR-RES-0001'),
    CURRENT_TIMESTAMP(0), 272.50, 25.60, 298.10,
    'Factura de prueba para integración API', 'WEB', 'EMITIDA',
    FALSE, CURRENT_TIMESTAMP(0), 'seed_admin', 'MS.Reservas'
WHERE NOT EXISTS (SELECT 1 FROM reservas.facturas WHERE numero_factura = 'FAC-0001');
