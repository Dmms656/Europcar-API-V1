-- =====================================================
-- MS.Seguridad - Desactivar RLS en security / audit
-- =====================================================
-- Supabase (y a veces el dashboard) puede activar Row Level Security
-- en tablas nuevas. El rol `postgres` del SQL Editor lo ignora; los roles
-- `ms_seguridad` no: el SELECT del login devuelve 0 filas y la API responde
-- "Credenciales inválidas" aunque el hash sea correcto.
--
-- Este proyecto ya aísla datos con roles dedicados + GRANT (99_supabase_grants.sql),
-- no con políticas RLS por usuario Supabase Auth. Por eso conviene RLS OFF
-- en estos schemas de aplicación.
--
-- Ejecuta UNA vez en el SQL Editor (Primary, rol postgres) después de DDL + seed.
-- =====================================================

ALTER TABLE security.api_clientes     DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.permisos         DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.roles            DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.roles_permisos   DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.sesiones         DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.usuarios_app     DISABLE ROW LEVEL SECURITY;
ALTER TABLE security.usuarios_roles   DISABLE ROW LEVEL SECURITY;

ALTER TABLE audit.aud_eventos         DISABLE ROW LEVEL SECURITY;
ALTER TABLE audit.aud_intentos_login  DISABLE ROW LEVEL SECURITY;
