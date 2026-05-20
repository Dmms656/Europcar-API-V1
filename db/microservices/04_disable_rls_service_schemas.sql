-- =====================================================
-- Desactivar RLS en schemas de microservicios (catalogo, localizaciones, clientes, reservas)
-- =====================================================
-- Los roles ms_* no son "supabase auth users"; con RLS ON y sin políticas,
-- SELECT/INSERT devuelven 0 filas o fallan. Ejecutar como postgres en SQL Editor.
-- Idempotente: repetir no rompe nada.
-- =====================================================

-- catalogo
ALTER TABLE IF EXISTS catalogo.marca_vehiculos      DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS catalogo.categoria_vehiculos  DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS catalogo.extras               DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS catalogo.vehiculos            DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS catalogo.mantenimientos       DISABLE ROW LEVEL SECURITY;

-- localizaciones
ALTER TABLE IF EXISTS localizaciones.paises                  DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS localizaciones.ciudades                DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS localizaciones.localizaciones          DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS localizaciones.localizacion_extra_stock  DISABLE ROW LEVEL SECURITY;

-- clientes (requerido para POST /api/v2/booking/reservas)
ALTER TABLE IF EXISTS clientes.clientes    DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS clientes.conductores DISABLE ROW LEVEL SECURITY;

-- reservas
ALTER TABLE IF EXISTS reservas.reservas            DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.reserva_conductores DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.reserva_extras      DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.contratos         DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.checkin_out         DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.pagos             DISABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS reservas.facturas          DISABLE ROW LEVEL SECURITY;

-- Indices de apoyo para upsert rapido (idempotente)
CREATE INDEX IF NOT EXISTS IX_clientes_identificacion
    ON clientes.clientes (tipo_identificacion, numero_identificacion)
    WHERE es_eliminado = false;

CREATE INDEX IF NOT EXISTS IX_conductores_cliente_ident
    ON clientes.conductores (id_cliente, tipo_identificacion, numero_identificacion)
    WHERE es_eliminado = false;
