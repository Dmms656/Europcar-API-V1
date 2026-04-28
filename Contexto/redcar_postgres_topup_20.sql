-- ==========================================================
-- RedCar / Europcar - Top-up de datos hasta 20 registros
-- Script incremental e idempotente (no borra ni reemplaza datos).
-- Diseñado para ejecutarse DESPUES de:
--   1) DDL/logica
--   2) redcar_postgres_seed_corregido_v2.sql
-- ==========================================================

SELECT set_config('app.current_user', 'seed_topup', true);
SELECT set_config('app.current_ip', '127.0.0.1', true);
SELECT set_config('app.current_origin', 'DB', true);

-- ----------------------------------------------------------
-- CATALOGOS BASE
-- ----------------------------------------------------------

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.paises WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'X' || LPAD((v_count + i)::TEXT, 1, '0');
        IF LENGTH(v_codigo) > 2 THEN
            v_codigo := CHR(65 + ((v_count + i) % 26)) || CHR(65 + ((v_count + i + 7) % 26));
        END IF;

        INSERT INTO rental.paises (
            codigo_iso2, nombre_pais, estado_pais, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            'Pais Topup ' || LPAD((v_count + i)::TEXT, 2, '0'),
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_iso2) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.paises WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_pais INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.ciudades WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT p.id_pais
          INTO v_id_pais
          FROM rental.paises p
         WHERE p.es_eliminado = FALSE
         ORDER BY p.id_pais
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.paises WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        INSERT INTO rental.ciudades (
            id_pais, nombre_ciudad, estado_ciudad, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_id_pais,
            'Ciudad Topup ' || LPAD((v_count + i)::TEXT, 3, '0'),
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (id_pais, nombre_ciudad) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.ciudades WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_ciudad INT;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.localizaciones WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT c.id_ciudad
          INTO v_id_ciudad
          FROM rental.ciudades c
         WHERE c.es_eliminado = FALSE
         ORDER BY c.id_ciudad
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.ciudades WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        v_codigo := 'LOC-TP-' || LPAD((v_count + i)::TEXT, 4, '0');

        INSERT INTO rental.localizaciones (
            codigo_localizacion, nombre_localizacion, id_ciudad,
            direccion_localizacion, telefono_contacto, correo_contacto,
            horario_atencion, zona_horaria, latitud, longitud,
            estado_localizacion, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            'Localizacion Topup ' || LPAD((v_count + i)::TEXT, 3, '0'),
            v_id_ciudad,
            'Direccion Topup ' || (v_count + i),
            '09' || LPAD((10000000 + v_count + i)::TEXT, 8, '0'),
            'loc.topup.' || (v_count + i) || '@redcar.local',
            'Lunes a Domingo 08:00 - 18:00',
            'America/Guayaquil',
            (-2 + ((v_count + i) % 4))::DECIMAL(9,6),
            (-79 + ((v_count + i) % 3))::DECIMAL(9,6),
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_localizacion) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.localizaciones WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.marca_vehiculos WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'MRK' || LPAD((v_count + i)::TEXT, 2, '0');
        INSERT INTO rental.marca_vehiculos (
            codigo_marca, nombre_marca, descripcion_marca,
            estado_marca, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            'Marca Topup ' || LPAD((v_count + i)::TEXT, 2, '0'),
            'Marca generada para topup',
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_marca) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.marca_vehiculos WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.categoria_vehiculos WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'CAT' || LPAD((v_count + i)::TEXT, 2, '0');

        INSERT INTO rental.categoria_vehiculos (
            codigo_categoria, nombre_categoria, descripcion_categoria,
            kilometraje_ilimitado, limite_km_dia, cargo_km_excedente,
            estado_categoria, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            'Categoria Topup ' || LPAD((v_count + i)::TEXT, 2, '0'),
            'Categoria generada para topup',
            CASE WHEN ((v_count + i) % 2 = 0) THEN TRUE ELSE FALSE END,
            CASE WHEN ((v_count + i) % 2 = 0) THEN NULL ELSE 200 END,
            CASE WHEN ((v_count + i) % 2 = 0) THEN NULL ELSE 0.35 END,
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_categoria) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.categoria_vehiculos WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.extras WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'EXT-' || LPAD((v_count + i)::TEXT, 4, '0');

        INSERT INTO rental.extras (
            codigo_extra, nombre_extra, descripcion_extra, tipo_extra, requiere_stock,
            valor_fijo, estado_extra, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            'Extra Topup ' || LPAD((v_count + i)::TEXT, 3, '0'),
            'Extra generado para pruebas de volumen',
            CASE ((v_count + i) % 4)
                WHEN 0 THEN 'SERVICIO'
                WHEN 1 THEN 'FISICO'
                WHEN 2 THEN 'SEGURO'
                ELSE 'OTRO'
            END,
            ((v_count + i) % 3 = 0),
            ROUND((3 + ((v_count + i) % 15))::NUMERIC, 2),
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_extra) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.extras WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_loc INT;
    v_id_extra INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.localizacion_extra_stock WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT l.id_localizacion
          INTO v_id_loc
          FROM rental.localizaciones l
         WHERE l.es_eliminado = FALSE
         ORDER BY l.id_localizacion
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.localizaciones WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT e.id_extra
          INTO v_id_extra
          FROM rental.extras e
         WHERE e.es_eliminado = FALSE
           AND e.requiere_stock = TRUE
         ORDER BY e.id_extra
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.extras WHERE es_eliminado = FALSE AND requiere_stock = TRUE), 1))
         LIMIT 1;

        IF v_id_loc IS NOT NULL AND v_id_extra IS NOT NULL THEN
            INSERT INTO rental.localizacion_extra_stock (
                id_localizacion, id_extra, stock_disponible, stock_reservado,
                estado_stock, es_eliminado, creado_por_usuario, origen_registro
            )
            VALUES (
                v_id_loc,
                v_id_extra,
                2 + ((v_count + i) % 10),
                0,
                'ACT',
                FALSE,
                'seed_topup',
                'TOPUP'
            )
            ON CONFLICT (id_localizacion, id_extra) DO NOTHING;
        END IF;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.localizacion_extra_stock WHERE es_eliminado = FALSE;
        IF i > 200 THEN EXIT; END IF;
    END LOOP;
END $$;

-- ----------------------------------------------------------
-- CLIENTES / CONDUCTORES / VEHICULOS
-- ----------------------------------------------------------

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.clientes WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'CLI-' || LPAD((v_count + i)::TEXT, 4, '0');

        INSERT INTO rental.clientes (
            codigo_cliente, tipo_identificacion, numero_identificacion,
            cli_nombre1, cli_nombre2, cli_apellido1, cli_apellido2,
            fecha_nacimiento, cli_telefono, cli_correo, direccion_principal,
            estado_cliente, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            CASE WHEN ((v_count + i) % 4 = 0) THEN 'PAS' ELSE 'CED' END,
            CASE
                WHEN ((v_count + i) % 4 = 0) THEN 'P' || LPAD((90000000 + v_count + i)::TEXT, 8, '0')
                ELSE LPAD((1700000000 + v_count + i)::TEXT, 10, '0')
            END,
            'Nombre' || (v_count + i),
            NULL,
            'Apellido' || (v_count + i),
            NULL,
            DATE '1985-01-01' + ((v_count + i) * INTERVAL '200 day'),
            '09' || LPAD((30000000 + v_count + i)::TEXT, 8, '0'),
            'cliente.topup.' || (v_count + i) || '@demo.local',
            'Direccion cliente topup ' || (v_count + i),
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_cliente) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.clientes WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_codigo TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.conductores WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        v_codigo := 'CON-' || LPAD((v_count + i)::TEXT, 4, '0');

        INSERT INTO rental.conductores (
            codigo_conductor, tipo_identificacion, numero_identificacion,
            con_nombre1, con_nombre2, con_apellido1, con_apellido2,
            numero_licencia, fecha_vencimiento_licencia, edad_conductor,
            con_telefono, con_correo,
            estado_conductor, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_codigo,
            CASE WHEN ((v_count + i) % 3 = 0) THEN 'PAS' ELSE 'CED' END,
            CASE
                WHEN ((v_count + i) % 3 = 0) THEN 'P' || LPAD((91000000 + v_count + i)::TEXT, 8, '0')
                ELSE LPAD((1800000000 + v_count + i)::TEXT, 10, '0')
            END,
            'ConNombre' || (v_count + i),
            NULL,
            'ConApellido' || (v_count + i),
            NULL,
            'LIC-TOP-' || LPAD((v_count + i)::TEXT, 5, '0'),
            CURRENT_DATE + INTERVAL '900 day',
            22 + ((v_count + i) % 30),
            '09' || LPAD((40000000 + v_count + i)::TEXT, 8, '0'),
            'conductor.topup.' || (v_count + i) || '@demo.local',
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_conductor) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.conductores WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_marca INT;
    v_id_categoria INT;
    v_id_loc INT;
    v_codigo TEXT;
    v_placa TEXT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.vehiculos WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT m.id_marca INTO v_id_marca
          FROM rental.marca_vehiculos m
         WHERE m.es_eliminado = FALSE
         ORDER BY m.id_marca
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.marca_vehiculos WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT c.id_categoria INTO v_id_categoria
          FROM rental.categoria_vehiculos c
         WHERE c.es_eliminado = FALSE
         ORDER BY c.id_categoria
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.categoria_vehiculos WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT l.id_localizacion INTO v_id_loc
          FROM rental.localizaciones l
         WHERE l.es_eliminado = FALSE
         ORDER BY l.id_localizacion
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.localizaciones WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        v_codigo := 'VEH-' || LPAD((v_count + i)::TEXT, 4, '0');
        v_placa := 'PTP-' || LPAD((v_count + i)::TEXT, 4, '0');

        INSERT INTO rental.vehiculos (
            codigo_interno_vehiculo, placa_vehiculo, id_marca, id_categoria,
            modelo_vehiculo, anio_fabricacion, color_vehiculo, tipo_combustible,
            tipo_transmision, capacidad_pasajeros, capacidad_maletas, numero_puertas,
            localizacion_actual, precio_base_dia, kilometraje_actual, aire_acondicionado,
            estado_operativo, observaciones_generales, imagen_referencial_url,
            estado_vehiculo, es_eliminado, origen_registro, creado_por_usuario
        )
        VALUES (
            v_codigo,
            v_placa,
            v_id_marca,
            v_id_categoria,
            'Modelo Topup ' || LPAD((v_count + i)::TEXT, 3, '0'),
            (2020 + ((v_count + i) % 6))::SMALLINT,
            CASE ((v_count + i) % 5)
                WHEN 0 THEN 'Blanco'
                WHEN 1 THEN 'Negro'
                WHEN 2 THEN 'Gris'
                WHEN 3 THEN 'Azul'
                ELSE 'Plata'
            END,
            CASE ((v_count + i) % 3)
                WHEN 0 THEN 'GASOLINA'
                WHEN 1 THEN 'HIBRIDO'
                ELSE 'DIESEL'
            END,
            CASE WHEN ((v_count + i) % 2 = 0) THEN 'AUTOMATICA' ELSE 'MANUAL' END,
            4 + ((v_count + i) % 3),
            2 + ((v_count + i) % 3),
            4,
            v_id_loc,
            ROUND((40 + ((v_count + i) % 80))::NUMERIC, 2),
            1000 + ((v_count + i) * 700),
            TRUE,
            'DISPONIBLE',
            'Vehiculo generado por topup',
            'images/vehiculos/topup_' || LPAD((v_count + i)::TEXT, 3, '0') || '.jpg',
            'ACT',
            FALSE,
            'TOPUP',
            'seed_topup'
        )
        ON CONFLICT (codigo_interno_vehiculo) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.vehiculos WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

-- ----------------------------------------------------------
-- SEGURIDAD
-- ----------------------------------------------------------

DO $$
DECLARE
    v_count INT;
    i INT := 1;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.roles WHERE estado_rol = 'ACT';
    WHILE v_count < 20 LOOP
        INSERT INTO security.roles (
            nombre_rol, descripcion_rol, es_sistema, estado_rol, creado_por_usuario
        )
        VALUES (
            'TOPUP_ROLE_' || LPAD((v_count + i)::TEXT, 3, '0'),
            'Rol generado para topup',
            FALSE,
            'ACT',
            'seed_topup'
        )
        ON CONFLICT (nombre_rol) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.roles WHERE estado_rol = 'ACT';
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.permisos WHERE estado_permiso = 'ACT';
    WHILE v_count < 20 LOOP
        INSERT INTO security.permisos (
            codigo_permiso, modulo, accion, descripcion_permiso, estado_permiso, creado_por_usuario
        )
        VALUES (
            'TOPUP_PERM_' || LPAD((v_count + i)::TEXT, 3, '0'),
            'TOPUP',
            'READ',
            'Permiso generado para topup',
            'ACT',
            'seed_topup'
        )
        ON CONFLICT (codigo_permiso) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.permisos WHERE estado_permiso = 'ACT';
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_rol INT;
    v_id_permiso INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.roles_permisos;
    WHILE v_count < 20 LOOP
        SELECT r.id_rol INTO v_id_rol
          FROM security.roles r
         WHERE r.estado_rol = 'ACT'
         ORDER BY r.id_rol
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM security.roles WHERE estado_rol = 'ACT'), 1))
         LIMIT 1;

        SELECT p.id_permiso INTO v_id_permiso
          FROM security.permisos p
         WHERE p.estado_permiso = 'ACT'
         ORDER BY p.id_permiso
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM security.permisos WHERE estado_permiso = 'ACT'), 1))
         LIMIT 1;

        INSERT INTO security.roles_permisos (
            id_rol, id_permiso, estado_rol_permiso, creado_por_usuario
        )
        VALUES (
            v_id_rol, v_id_permiso, 'ACT', 'seed_topup'
        )
        ON CONFLICT (id_rol, id_permiso) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.roles_permisos;
        IF i > 300 THEN EXIT; END IF;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_cliente INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.usuarios_app WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT c.id_cliente INTO v_id_cliente
          FROM rental.clientes c
         WHERE c.es_eliminado = FALSE
         ORDER BY c.id_cliente
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.clientes WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        INSERT INTO security.usuarios_app (
            username, correo, password_hash, password_salt, password_hint,
            requiere_cambio_password, estado_usuario, es_eliminado, activo,
            intentos_fallidos, id_cliente, fecha_registro_utc, creado_por_usuario
        )
        SELECT
            'topup.user.' || LPAD((v_count + i)::TEXT, 3, '0'),
            'topup.user.' || LPAD((v_count + i)::TEXT, 3, '0') || '@redcar.local',
            crypt('12345', s.salt),
            s.salt,
            'topup',
            FALSE,
            'ACT',
            FALSE,
            TRUE,
            0,
            v_id_cliente,
            CURRENT_TIMESTAMP(0),
            'seed_topup'
        FROM (SELECT gen_salt('bf') AS salt) s
        ON CONFLICT (username) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.usuarios_app WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_usuario INT;
    v_id_rol INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.usuarios_roles WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT u.id_usuario INTO v_id_usuario
          FROM security.usuarios_app u
         WHERE u.es_eliminado = FALSE
         ORDER BY u.id_usuario
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM security.usuarios_app WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT r.id_rol INTO v_id_rol
          FROM security.roles r
         WHERE r.estado_rol = 'ACT'
         ORDER BY r.id_rol
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM security.roles WHERE estado_rol = 'ACT'), 1))
         LIMIT 1;

        INSERT INTO security.usuarios_roles (
            id_usuario, id_rol, estado_usuario_rol, es_eliminado, activo, creado_por_usuario
        )
        VALUES (
            v_id_usuario, v_id_rol, 'ACT', FALSE, TRUE, 'seed_topup'
        )
        ON CONFLICT (id_usuario, id_rol) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.usuarios_roles WHERE es_eliminado = FALSE;
        IF i > 300 THEN EXIT; END IF;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
BEGIN
    SELECT COUNT(*) INTO v_count FROM security.api_clientes WHERE estado_api_cliente = 'ACT';
    WHILE v_count < 20 LOOP
        INSERT INTO security.api_clientes (
            codigo_cliente_api, nombre_cliente_api, tipo_autenticacion,
            api_key_hash, client_secret_hash, permite_consulta_disponibilidad,
            permite_sincronizacion_reservas, estado_api_cliente,
            fecha_rotacion_credenciales_utc, creado_por_usuario
        )
        VALUES (
            'TOPUP-API-' || LPAD((v_count + i)::TEXT, 3, '0'),
            'Topup API Cliente ' || LPAD((v_count + i)::TEXT, 3, '0'),
            'JWT',
            encode(digest('topup-key-' || (v_count + i), 'sha256'), 'hex'),
            encode(digest('topup-secret-' || (v_count + i), 'sha256'), 'hex'),
            TRUE,
            TRUE,
            'ACT',
            CURRENT_TIMESTAMP(0),
            'seed_topup'
        )
        ON CONFLICT (codigo_cliente_api) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM security.api_clientes WHERE estado_api_cliente = 'ACT';
    END LOOP;
END $$;

-- ----------------------------------------------------------
-- OPERACION: RESERVAS / CONTRATOS / PAGOS / FACTURAS / MANTENIMIENTOS
-- ----------------------------------------------------------

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_cliente INT;
    v_id_vehiculo INT;
    v_id_loc INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.reservas WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT c.id_cliente INTO v_id_cliente
          FROM rental.clientes c
         WHERE c.es_eliminado = FALSE AND c.estado_cliente = 'ACT'
         ORDER BY c.id_cliente
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.clientes WHERE es_eliminado = FALSE AND estado_cliente = 'ACT'), 1))
         LIMIT 1;

        SELECT v.id_vehiculo, v.localizacion_actual
          INTO v_id_vehiculo, v_id_loc
          FROM rental.vehiculos v
         WHERE v.es_eliminado = FALSE AND v.estado_vehiculo = 'ACT'
         ORDER BY v.id_vehiculo
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.vehiculos WHERE es_eliminado = FALSE AND estado_vehiculo = 'ACT'), 1))
         LIMIT 1;

        INSERT INTO rental.reservas (
            codigo_reserva, id_cliente, id_vehiculo,
            id_localizacion_recogida, id_localizacion_devolucion,
            canal_reserva, fecha_hora_recogida, fecha_hora_devolucion,
            subtotal, valor_impuestos, valor_extras, valor_deposito_garantia,
            cargo_one_way, total, codigo_confirmacion, estado_reserva,
            requiere_hold, creado_por_usuario, origen_registro
        )
        VALUES (
            'RES-' || LPAD((v_count + i)::TEXT, 4, '0'),
            v_id_cliente,
            v_id_vehiculo,
            v_id_loc,
            v_id_loc,
            CASE ((v_count + i) % 3) WHEN 0 THEN 'WEB' WHEN 1 THEN 'APP' ELSE 'POS' END,
            CURRENT_TIMESTAMP(0) + ((v_count + i) * INTERVAL '1 day'),
            CURRENT_TIMESTAMP(0) + ((v_count + i) * INTERVAL '1 day') + INTERVAL '2 day',
            100 + ((v_count + i) * 3),
            ROUND((100 + ((v_count + i) * 3)) * 0.12, 2),
            0,
            150.00,
            0,
            ROUND((100 + ((v_count + i) * 3)) * 1.12, 2),
            'CNF-' || LPAD((v_count + i)::TEXT, 4, '0'),
            'CONFIRMADA',
            TRUE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_reserva) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.reservas WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_reserva INT;
    v_id_conductor INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.reserva_conductores WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT r.id_reserva INTO v_id_reserva
          FROM rental.reservas r
         WHERE r.es_eliminado = FALSE
         ORDER BY r.id_reserva
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.reservas WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT c.id_conductor INTO v_id_conductor
          FROM rental.conductores c
         WHERE c.es_eliminado = FALSE AND c.estado_conductor = 'ACT'
         ORDER BY c.id_conductor
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.conductores WHERE es_eliminado = FALSE AND estado_conductor = 'ACT'), 1))
         LIMIT 1;

        INSERT INTO rental.reserva_conductores (
            id_reserva, id_conductor, tipo_conductor, es_principal, cargo_conductor_joven,
            estado_reserva_conductor, es_eliminado, creado_por_usuario, origen_registro
        )
        VALUES (
            v_id_reserva,
            v_id_conductor,
            'TITULAR',
            TRUE,
            0,
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.reserva_conductores WHERE es_eliminado = FALSE;
        IF i > 500 THEN EXIT; END IF;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_reserva INT;
    v_id_extra INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.reserva_extras WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT r.id_reserva INTO v_id_reserva
          FROM rental.reservas r
         WHERE r.es_eliminado = FALSE
         ORDER BY r.id_reserva
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.reservas WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        SELECT e.id_extra INTO v_id_extra
          FROM rental.extras e
         WHERE e.es_eliminado = FALSE AND e.estado_extra = 'ACT'
         ORDER BY e.id_extra
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.extras WHERE es_eliminado = FALSE AND estado_extra = 'ACT'), 1))
         LIMIT 1;

        INSERT INTO rental.reserva_extras (
            id_reserva, id_extra, cantidad, valor_unitario_extra, subtotal_extra,
            estado_reserva_extra, es_eliminado, creado_por_usuario, origen_registro
        )
        SELECT
            v_id_reserva,
            v_id_extra,
            1,
            e.valor_fijo,
            e.valor_fijo,
            'ACT',
            FALSE,
            'seed_topup',
            'TOPUP'
        FROM rental.extras e
        WHERE e.id_extra = v_id_extra
        ON CONFLICT DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.reserva_extras WHERE es_eliminado = FALSE;
        IF i > 500 THEN EXIT; END IF;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    v_id_reserva INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.contratos WHERE es_eliminado = FALSE;
    IF v_count < 20 THEN
        FOR v_id_reserva IN
            SELECT r.id_reserva
            FROM rental.reservas r
            LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
            WHERE r.es_eliminado = FALSE
              AND r.estado_reserva = 'CONFIRMADA'
              AND c.id_contrato IS NULL
            ORDER BY r.id_reserva
        LOOP
            EXIT WHEN (SELECT COUNT(*) FROM rental.contratos WHERE es_eliminado = FALSE) >= 20;

            PERFORM rental.fn_generar_contrato(
                v_id_reserva,
                'seed_topup',
                (SELECT COALESCE(v.kilometraje_actual, 0)
                   FROM rental.vehiculos v
                   JOIN rental.reservas r2 ON r2.id_vehiculo = v.id_vehiculo
                  WHERE r2.id_reserva = v_id_reserva),
                100.00,
                'docs/contratos/topup_' || v_id_reserva || '.pdf',
                'Contrato generado por topup',
                'TOPUP'
            );
        END LOOP;
    END IF;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_reserva INT;
    v_id_contrato INT;
    v_id_cliente INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.pagos WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT r.id_reserva, c.id_contrato, r.id_cliente
          INTO v_id_reserva, v_id_contrato, v_id_cliente
          FROM rental.reservas r
          LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
         WHERE r.es_eliminado = FALSE
         ORDER BY r.id_reserva
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.reservas WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        INSERT INTO rental.pagos (
            codigo_pago, id_reserva, id_contrato, id_cliente,
            tipo_pago, metodo_pago, estado_pago, referencia_externa,
            monto, moneda, fecha_pago_utc, observaciones_pago,
            creado_por_usuario, origen_registro
        )
        VALUES (
            'PAG-' || LPAD((v_count + i)::TEXT, 4, '0'),
            v_id_reserva,
            v_id_contrato,
            v_id_cliente,
            'COBRO',
            CASE ((v_count + i) % 3) WHEN 0 THEN 'TARJETA' WHEN 1 THEN 'TRANSFERENCIA' ELSE 'EFECTIVO' END,
            'APROBADO',
            'TRX-TOP-' || LPAD((v_count + i)::TEXT, 6, '0'),
            ROUND((80 + ((v_count + i) % 120))::NUMERIC, 2),
            'USD',
            CURRENT_TIMESTAMP(0),
            'Pago generado por topup',
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (codigo_pago) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.pagos WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_reserva INT;
    v_id_contrato INT;
    v_id_cliente INT;
    v_subtotal NUMERIC(12,2);
    v_iva NUMERIC(12,2);
    v_total NUMERIC(12,2);
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.facturas WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT r.id_reserva, c.id_contrato, r.id_cliente, r.subtotal
          INTO v_id_reserva, v_id_contrato, v_id_cliente, v_subtotal
          FROM rental.reservas r
          LEFT JOIN rental.contratos c ON c.id_reserva = r.id_reserva AND c.es_eliminado = FALSE
         WHERE r.es_eliminado = FALSE
         ORDER BY r.id_reserva
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.reservas WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        v_iva := ROUND(COALESCE(v_subtotal, 100) * 0.12, 2);
        v_total := ROUND(COALESCE(v_subtotal, 100) + v_iva, 2);

        INSERT INTO rental.facturas (
            numero_factura, id_cliente, id_reserva, id_contrato,
            fecha_emision, subtotal, valor_iva, total,
            observaciones_factura, origen_canal_factura, estado_factura,
            es_eliminado, fecha_registro_utc, creado_por_usuario, servicio_origen
        )
        VALUES (
            'FAC-' || LPAD((v_count + i)::TEXT, 4, '0'),
            v_id_cliente,
            v_id_reserva,
            v_id_contrato,
            CURRENT_TIMESTAMP(0),
            COALESCE(v_subtotal, 100),
            v_iva,
            v_total,
            'Factura generada por topup',
            'WEB',
            'EMITIDA',
            FALSE,
            CURRENT_TIMESTAMP(0),
            'seed_topup',
            'TOPUP'
        )
        ON CONFLICT (numero_factura) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.facturas WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

DO $$
DECLARE
    v_count INT;
    i INT := 1;
    v_id_vehiculo INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM rental.mantenimientos WHERE es_eliminado = FALSE;
    WHILE v_count < 20 LOOP
        SELECT v.id_vehiculo
          INTO v_id_vehiculo
          FROM rental.vehiculos v
         WHERE v.es_eliminado = FALSE
         ORDER BY v.id_vehiculo
         OFFSET ((i - 1) % GREATEST((SELECT COUNT(*) FROM rental.vehiculos WHERE es_eliminado = FALSE), 1))
         LIMIT 1;

        INSERT INTO rental.mantenimientos (
            codigo_mantenimiento, id_vehiculo, tipo_mantenimiento,
            fecha_inicio_utc, fecha_fin_utc, kilometraje_mantenimiento,
            costo_mantenimiento, proveedor_taller, estado_mantenimiento,
            observaciones, creado_por_usuario
        )
        VALUES (
            'MNT-' || LPAD((v_count + i)::TEXT, 4, '0'),
            v_id_vehiculo,
            CASE ((v_count + i) % 2) WHEN 0 THEN 'PREVENTIVO' ELSE 'CORRECTIVO' END,
            CURRENT_TIMESTAMP(0) - ((v_count + i) * INTERVAL '3 day'),
            NULL,
            5000 + ((v_count + i) * 600),
            ROUND((60 + ((v_count + i) % 180))::NUMERIC, 2),
            'Taller Topup ' || ((v_count + i) % 5 + 1),
            CASE ((v_count + i) % 3) WHEN 0 THEN 'ABIERTO' ELSE 'CERRADO' END,
            'Mantenimiento generado por topup',
            'seed_topup'
        )
        ON CONFLICT (codigo_mantenimiento) DO NOTHING;

        i := i + 1;
        SELECT COUNT(*) INTO v_count FROM rental.mantenimientos WHERE es_eliminado = FALSE;
    END LOOP;
END $$;

-- ----------------------------------------------------------
-- RESUMEN FINAL
-- ----------------------------------------------------------

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

