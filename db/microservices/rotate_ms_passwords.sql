-- =====================================================
-- Rotar contraseñas de los 5 roles de aplicación (Supabase / PostgreSQL)
-- =====================================================
-- ANTES DE EJECUTAR:
--   1. Sustituye cada '__CAMBIA_ESTO_*__' por una contraseña fuerte (16+ caracteres).
--   2. Ejecuta TODO el script en el SQL Editor (rol postgres, Primary Database).
--   3. Actualiza en Render (y en .env local) las connection strings con la nueva
--      contraseña de CADA rol (cada microservicio usa su ms_* correspondiente;
--      el middleware usa ms_seguridad).
--   4. Redeploy o reinicia servicios para que tomen la nueva cadena.
--
-- No subas este archivo a git con contraseñas reales; revierte con git checkout
-- si pegaste secretos por error.
-- =====================================================

ALTER ROLE ms_seguridad      WITH PASSWORD '__CAMBIA_ESTO_SEGURIDAD__';
ALTER ROLE ms_catalogo       WITH PASSWORD '__CAMBIA_ESTO_CATALOGO__';
ALTER ROLE ms_localizaciones WITH PASSWORD '__CAMBIA_ESTO_LOCALIZACIONES__';
ALTER ROLE ms_clientes       WITH PASSWORD '__CAMBIA_ESTO_CLIENTES__';
ALTER ROLE ms_reservas       WITH PASSWORD '__CAMBIA_ESTO_RESERVAS__';

-- Verificación (solo comprueba que el rol existe; no muestra contraseña):
SELECT rolname,
       rolcanlogin AS puede_login,
       rolvaliduntil AS valido_hasta
FROM pg_roles
WHERE rolname IN (
    'ms_seguridad',
    'ms_catalogo',
    'ms_localizaciones',
    'ms_clientes',
    'ms_reservas'
)
ORDER BY rolname;
