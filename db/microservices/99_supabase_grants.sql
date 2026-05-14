-- =====================================================
-- SUPABASE - Roles DB y GRANTs por microservicio
-- =====================================================
-- Crea 5 usuarios de base de datos, uno por microservicio,
-- y les da acceso EXCLUSIVO a sus tablas. PostgreSQL
-- enforcea el aislamiento: cada role solo puede ver/operar
-- sobre el schema que le corresponde.
--
-- EJECUCION:
--   1. Ya ejecutaste los 5 DDLs y los 5 seeds (los schemas
--      y las tablas ya existen).
--   2. Antes de pegar este archivo en el SQL Editor de Supabase,
--      REEMPLAZA los 5 placeholders __CHANGE_ME_*__ por
--      contrasenias fuertes (16+ caracteres). Guarda esas
--      contrasenias en db/microservices/.env.
--   3. Ejecuta este archivo COMPLETO una sola vez.
--
-- Si tienes que rotar contrasenias mas adelante:
--   ALTER ROLE ms_catalogo WITH PASSWORD 'nueva';
-- =====================================================

-- =====================================================
-- 1. Crear los 5 roles de servicio (LOGIN, sin privilegios extra)
-- =====================================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ms_seguridad') THEN
        CREATE ROLE ms_seguridad WITH LOGIN PASSWORD 'G7kP2nQxY8wRzM3FvJtH6cBaL_9DhUNs';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ms_catalogo') THEN
        CREATE ROLE ms_catalogo WITH LOGIN PASSWORD 'T4mZ_xQ8hL2nVw9PyKbR.cF6gJaMHsUe';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ms_localizaciones') THEN
        CREATE ROLE ms_localizaciones WITH LOGIN PASSWORD '9HvNxBkY-R3mZqP2cW.tD4F8jLgKaUSh';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ms_clientes') THEN
        CREATE ROLE ms_clientes WITH LOGIN PASSWORD 'c8KxR_M2yPnZv4HbTqL.6gFaWjUNs9DH';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ms_reservas') THEN
        CREATE ROLE ms_reservas WITH LOGIN PASSWORD 'A2mZ.kQ8nVw9PyKbRc_F6gJxYsUHe-TL';
    END IF;
END $$;

-- Permitir conectarse a la base postgres (la unica que hay en Supabase)
GRANT CONNECT ON DATABASE postgres TO
    ms_seguridad, ms_catalogo, ms_localizaciones, ms_clientes, ms_reservas;

-- =====================================================
-- 2. MS.Seguridad -> schemas security + audit
-- =====================================================
GRANT USAGE ON SCHEMA security TO ms_seguridad;
GRANT USAGE ON SCHEMA audit    TO ms_seguridad;

GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA security TO ms_seguridad;
GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA audit    TO ms_seguridad;

GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA security TO ms_seguridad;
GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA audit    TO ms_seguridad;

ALTER DEFAULT PRIVILEGES IN SCHEMA security
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_seguridad;
ALTER DEFAULT PRIVILEGES IN SCHEMA security
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_seguridad;
ALTER DEFAULT PRIVILEGES IN SCHEMA audit
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_seguridad;
ALTER DEFAULT PRIVILEGES IN SCHEMA audit
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_seguridad;

-- =====================================================
-- 3. MS.Catalogo -> schema catalogo
-- =====================================================
GRANT USAGE ON SCHEMA catalogo TO ms_catalogo;

GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA catalogo TO ms_catalogo;
GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA catalogo TO ms_catalogo;

ALTER DEFAULT PRIVILEGES IN SCHEMA catalogo
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_catalogo;
ALTER DEFAULT PRIVILEGES IN SCHEMA catalogo
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_catalogo;

-- =====================================================
-- 4. MS.Localizaciones -> schema localizaciones
-- =====================================================
GRANT USAGE ON SCHEMA localizaciones TO ms_localizaciones;

GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA localizaciones TO ms_localizaciones;
GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA localizaciones TO ms_localizaciones;

ALTER DEFAULT PRIVILEGES IN SCHEMA localizaciones
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_localizaciones;
ALTER DEFAULT PRIVILEGES IN SCHEMA localizaciones
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_localizaciones;

-- =====================================================
-- 5. MS.Clientes -> schema clientes
-- =====================================================
GRANT USAGE ON SCHEMA clientes TO ms_clientes;

GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA clientes TO ms_clientes;
GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA clientes TO ms_clientes;

ALTER DEFAULT PRIVILEGES IN SCHEMA clientes
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_clientes;
ALTER DEFAULT PRIVILEGES IN SCHEMA clientes
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_clientes;

-- =====================================================
-- 6. MS.Reservas + Facturacion -> schema reservas
-- =====================================================
GRANT USAGE ON SCHEMA reservas TO ms_reservas;

GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES
    ON ALL TABLES IN SCHEMA reservas TO ms_reservas;
GRANT USAGE, SELECT, UPDATE
    ON ALL SEQUENCES IN SCHEMA reservas TO ms_reservas;

ALTER DEFAULT PRIVILEGES IN SCHEMA reservas
    GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER, REFERENCES ON TABLES TO ms_reservas;
ALTER DEFAULT PRIVILEGES IN SCHEMA reservas
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO ms_reservas;

-- =====================================================
-- 7. search_path por defecto (calidad de vida)
-- =====================================================
-- Cuando un MS se conecte, su search_path quedara apuntando a su(s)
-- schema(s). Asi en el codigo no hace falta calificar el nombre.
--
-- ms_seguridad: el login usa crypt()/gen_salt (pgcrypto). En Supabase la
-- extension suele instalarse en el schema "extensions"; sin el en el path,
-- Postgres responde 42883 "function crypt(...) does not exist".
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'extensions') THEN
    EXECUTE 'GRANT USAGE ON SCHEMA extensions TO ms_seguridad';
    EXECUTE 'ALTER ROLE ms_seguridad SET search_path = security, audit, public, extensions';
  ELSE
    EXECUTE 'ALTER ROLE ms_seguridad SET search_path = security, audit, public';
  END IF;
END $$;
ALTER ROLE ms_catalogo       SET search_path = catalogo, public;
ALTER ROLE ms_localizaciones SET search_path = localizaciones, public;
ALTER ROLE ms_clientes       SET search_path = clientes, public;
ALTER ROLE ms_reservas       SET search_path = reservas, public;

-- =====================================================
-- 8. Verificacion rapida (opcional - corre estas SELECTs)
-- =====================================================
-- Lista los roles creados:
--   SELECT rolname FROM pg_roles WHERE rolname LIKE 'ms_%';
--
-- Verifica los grants sobre un schema:
--   SELECT grantee, privilege_type
--   FROM information_schema.role_table_grants
--   WHERE table_schema = 'catalogo' AND grantee LIKE 'ms_%';
--
-- Comprobacion de aislamiento (ejecutar en una nueva sesion conectada
-- como ms_catalogo, deberia DAR ERROR de "permission denied"):
--   SELECT * FROM clientes.clientes LIMIT 1;
