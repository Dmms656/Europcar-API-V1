-- ==========================================================
-- RedCar / Europcar - Lógica de base de datos para PostgreSQL
-- Script 2: triggers, funciones y procedimientos auxiliares
-- Requiere ejecutar primero: redcar_postgres_ddl_corregido.sql
-- ==========================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS rental;
CREATE SCHEMA IF NOT EXISTS security;
CREATE SCHEMA IF NOT EXISTS audit;

-- ----------------------------------------------------------
-- 1) FUNCIONES GENÉRICAS DE AUDITORÍA Y CONCURRENCIA
-- ----------------------------------------------------------

CREATE OR REPLACE FUNCTION public.fn_touch_audit_fields()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'UPDATE' THEN
        IF EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = TG_TABLE_SCHEMA
              AND table_name = TG_TABLE_NAME
              AND column_name = 'fecha_modificacion_utc'
        ) THEN
            NEW.fecha_modificacion_utc := CURRENT_TIMESTAMP(0);
        END IF;

        IF EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = TG_TABLE_SCHEMA
              AND table_name = TG_TABLE_NAME
              AND column_name = 'row_version'
        ) THEN
            NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
        END IF;
    END IF;
    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION audit.fn_log_row_change()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_pk TEXT;
    v_old TEXT;
    v_new TEXT;
    v_user TEXT;
    v_ip TEXT;
    v_origin TEXT;
BEGIN
    v_user := COALESCE(current_setting('app.current_user', true), current_user);
    v_ip := current_setting('app.current_ip', true);
    v_origin := COALESCE(current_setting('app.current_origin', true), 'DB');

    IF TG_OP = 'INSERT' THEN
        v_new := row_to_json(NEW)::text;
        v_pk := COALESCE(v_new::jsonb ->> 'id_reserva',
                         v_new::jsonb ->> 'id_contrato',
                         v_new::jsonb ->> 'id_pago',
                         v_new::jsonb ->> 'id_factura',
                         v_new::jsonb ->> 'id_vehiculo',
                         v_new::jsonb ->> 'id_cliente',
                         v_new::jsonb ->> 'id_usuario',
                         v_new::jsonb ->> 'id_mantenimiento',
                         v_new::jsonb ->> 'id_check',
                         v_new::jsonb ->> 'id_aud_evento');

        INSERT INTO audit.AUD_EVENTOS (
            esquema_afectado, tabla_afectada, operacion, id_registro_afectado,
            datos_anteriores, datos_nuevos, usuario_app, login_bd, ip_origen,
            origen_evento, fecha_evento_utc, row_version
        )
        VALUES (
            TG_TABLE_SCHEMA, TG_TABLE_NAME, 'INSERT', v_pk,
            NULL, v_new, v_user, current_user, v_ip,
            v_origin, CURRENT_TIMESTAMP(0), 1
        );
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        v_old := row_to_json(OLD)::text;
        v_new := row_to_json(NEW)::text;
        v_pk := COALESCE(v_new::jsonb ->> 'id_reserva',
                         v_new::jsonb ->> 'id_contrato',
                         v_new::jsonb ->> 'id_pago',
                         v_new::jsonb ->> 'id_factura',
                         v_new::jsonb ->> 'id_vehiculo',
                         v_new::jsonb ->> 'id_cliente',
                         v_new::jsonb ->> 'id_usuario',
                         v_new::jsonb ->> 'id_mantenimiento',
                         v_new::jsonb ->> 'id_check',
                         v_new::jsonb ->> 'id_aud_evento');

        INSERT INTO audit.AUD_EVENTOS (
            esquema_afectado, tabla_afectada, operacion, id_registro_afectado,
            datos_anteriores, datos_nuevos, usuario_app, login_bd, ip_origen,
            origen_evento, fecha_evento_utc, row_version
        )
        VALUES (
            TG_TABLE_SCHEMA, TG_TABLE_NAME, 'UPDATE', v_pk,
            v_old, v_new, v_user, current_user, v_ip,
            v_origin, CURRENT_TIMESTAMP(0), 1
        );
        RETURN NEW;
    ELSE
        v_old := row_to_json(OLD)::text;
        v_pk := COALESCE(v_old::jsonb ->> 'id_reserva',
                         v_old::jsonb ->> 'id_contrato',
                         v_old::jsonb ->> 'id_pago',
                         v_old::jsonb ->> 'id_factura',
                         v_old::jsonb ->> 'id_vehiculo',
                         v_old::jsonb ->> 'id_cliente',
                         v_old::jsonb ->> 'id_usuario',
                         v_old::jsonb ->> 'id_mantenimiento',
                         v_old::jsonb ->> 'id_check',
                         v_old::jsonb ->> 'id_aud_evento');

        INSERT INTO audit.AUD_EVENTOS (
            esquema_afectado, tabla_afectada, operacion, id_registro_afectado,
            datos_anteriores, datos_nuevos, usuario_app, login_bd, ip_origen,
            origen_evento, fecha_evento_utc, row_version
        )
        VALUES (
            TG_TABLE_SCHEMA, TG_TABLE_NAME, 'DELETE', v_pk,
            v_old, NULL, v_user, current_user, v_ip,
            v_origin, CURRENT_TIMESTAMP(0), 1
        );
        RETURN OLD;
    END IF;
END;
$$;

-- ----------------------------------------------------------
-- 2) FUNCIONES DE NEGOCIO: RESERVAS, EXTRAS, CONTRATOS
-- ----------------------------------------------------------

CREATE OR REPLACE FUNCTION rental.fn_reservas_solapadas(
    p_id_vehiculo INT,
    p_fecha_inicio TIMESTAMPTZ,
    p_fecha_fin TIMESTAMPTZ,
    p_id_reserva_actual INT DEFAULT NULL
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_existe BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM rental.RESERVAS r
        WHERE r.id_vehiculo = p_id_vehiculo
          AND r.estado_reserva IN ('PENDIENTE', 'CONFIRMADA', 'EN_CURSO')
          AND (p_id_reserva_actual IS NULL OR r.id_reserva <> p_id_reserva_actual)
          AND tstzrange(r.fecha_hora_recogida, r.fecha_hora_devolucion, '[)') &&
              tstzrange(p_fecha_inicio, p_fecha_fin, '[)')
    )
    INTO v_existe;

    RETURN v_existe;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_validar_reserva()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_estado_vehiculo VARCHAR(20);
    v_fecha_nacimiento DATE;
BEGIN
    IF rental.fn_reservas_solapadas(NEW.id_vehiculo, NEW.fecha_hora_recogida, NEW.fecha_hora_devolucion, COALESCE(NEW.id_reserva, NULL)) THEN
        RAISE EXCEPTION 'El vehículo % ya tiene una reserva activa en el rango solicitado', NEW.id_vehiculo;
    END IF;

    SELECT estado_operativo INTO v_estado_vehiculo
    FROM rental.VEHICULOS
    WHERE id_vehiculo = NEW.id_vehiculo;

    IF v_estado_vehiculo IN ('MANTENIMIENTO', 'TALLER', 'ALQUILADO', 'FUERA_SERVICIO') THEN
        RAISE EXCEPTION 'El vehículo % no está disponible. Estado operativo actual: %', NEW.id_vehiculo, v_estado_vehiculo;
    END IF;

    SELECT fecha_nacimiento INTO v_fecha_nacimiento
    FROM rental.CLIENTES
    WHERE id_cliente = NEW.id_cliente;

    IF age(NEW.fecha_hora_recogida::date, v_fecha_nacimiento) < INTERVAL '21 years' THEN
        RAISE EXCEPTION 'El cliente % no cumple la edad mínima de 21 años para alquilar', NEW.id_cliente;
    END IF;

    NEW.total := COALESCE(NEW.subtotal, 0)
               + COALESCE(NEW.valor_impuestos, 0)
               + COALESCE(NEW.valor_extras, 0)
               + COALESCE(NEW.cargo_one_way, 0);

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_sync_reserva_extras_total()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_id_reserva INT;
BEGIN
    v_id_reserva := COALESCE(NEW.id_reserva, OLD.id_reserva);

    UPDATE rental.RESERVAS r
    SET valor_extras = COALESCE((
            SELECT SUM(re.subtotal_extra)
            FROM rental.RESERVA_EXTRAS re
            WHERE re.id_reserva = v_id_reserva
              AND re.es_eliminado = FALSE
              AND re.estado_reserva_extra = 'ACT'
        ), 0),
        total = COALESCE(subtotal, 0)
              + COALESCE(valor_impuestos, 0)
              + COALESCE((
                    SELECT SUM(re.subtotal_extra)
                    FROM rental.RESERVA_EXTRAS re
                    WHERE re.id_reserva = v_id_reserva
                      AND re.es_eliminado = FALSE
                      AND re.estado_reserva_extra = 'ACT'
                ), 0)
              + COALESCE(cargo_one_way, 0),
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        modificado_por_usuario = COALESCE(current_setting('app.current_user', true), current_user),
        row_version = row_version + 1
    WHERE r.id_reserva = v_id_reserva;

    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_validar_stock_extra()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_requiere_stock BOOLEAN;
    v_stock_disponible INT;
    v_localizacion INT;
BEGIN
    SELECT e.requiere_stock, r.id_localizacion_recogida
    INTO v_requiere_stock, v_localizacion
    FROM rental.EXTRAS e
    JOIN rental.RESERVAS r ON r.id_reserva = NEW.id_reserva
    WHERE e.id_extra = NEW.id_extra;

    IF COALESCE(v_requiere_stock, FALSE) = FALSE THEN
        RETURN NEW;
    END IF;

    SELECT (stock_disponible - stock_reservado)
    INTO v_stock_disponible
    FROM rental.LOCALIZACION_EXTRA_STOCK
    WHERE id_localizacion = v_localizacion
      AND id_extra = NEW.id_extra;

    IF COALESCE(v_stock_disponible, 0) < NEW.cantidad THEN
        RAISE EXCEPTION 'No existe stock suficiente del extra % en localización %', NEW.id_extra, v_localizacion;
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_actualizar_stock_extra()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_requiere_stock BOOLEAN;
    v_localizacion INT;
    v_delta INT := 0;
    v_extra INT;
BEGIN
    IF TG_OP = 'INSERT' THEN
        v_extra := NEW.id_extra;
        SELECT e.requiere_stock, r.id_localizacion_recogida
        INTO v_requiere_stock, v_localizacion
        FROM rental.EXTRAS e
        JOIN rental.RESERVAS r ON r.id_reserva = NEW.id_reserva
        WHERE e.id_extra = NEW.id_extra;
        v_delta := NEW.cantidad;
    ELSIF TG_OP = 'UPDATE' THEN
        v_extra := NEW.id_extra;
        SELECT e.requiere_stock, r.id_localizacion_recogida
        INTO v_requiere_stock, v_localizacion
        FROM rental.EXTRAS e
        JOIN rental.RESERVAS r ON r.id_reserva = NEW.id_reserva
        WHERE e.id_extra = NEW.id_extra;
        v_delta := NEW.cantidad - OLD.cantidad;
    ELSE
        v_extra := OLD.id_extra;
        SELECT e.requiere_stock, r.id_localizacion_recogida
        INTO v_requiere_stock, v_localizacion
        FROM rental.EXTRAS e
        JOIN rental.RESERVAS r ON r.id_reserva = OLD.id_reserva
        WHERE e.id_extra = OLD.id_extra;
        v_delta := -OLD.cantidad;
    END IF;

    IF COALESCE(v_requiere_stock, FALSE) = FALSE THEN
        RETURN COALESCE(NEW, OLD);
    END IF;

    UPDATE rental.LOCALIZACION_EXTRA_STOCK
    SET stock_reservado = stock_reservado + v_delta,
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        modificado_por_usuario = COALESCE(current_setting('app.current_user', true), current_user),
        row_version = row_version + 1
    WHERE id_localizacion = v_localizacion
      AND id_extra = v_extra;

    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_validar_contrato()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_reserva RECORD;
BEGIN
    SELECT *
    INTO v_reserva
    FROM rental.RESERVAS
    WHERE id_reserva = NEW.id_reserva;

    IF v_reserva.id_cliente <> NEW.id_cliente THEN
        RAISE EXCEPTION 'El cliente del contrato no coincide con la reserva';
    END IF;

    IF v_reserva.id_vehiculo <> NEW.id_vehiculo THEN
        RAISE EXCEPTION 'El vehículo del contrato no coincide con la reserva';
    END IF;

    IF v_reserva.estado_reserva NOT IN ('CONFIRMADA', 'EN_CURSO') THEN
        RAISE EXCEPTION 'Solo se puede generar contrato para reservas confirmadas o en curso';
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_post_contrato_creado()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE rental.RESERVAS
    SET estado_reserva = 'EN_CURSO',
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        modificado_por_usuario = COALESCE(current_setting('app.current_user', true), current_user),
        row_version = row_version + 1
    WHERE id_reserva = NEW.id_reserva;

    UPDATE rental.VEHICULOS
    SET estado_operativo = 'ALQUILADO',
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        modificado_por_usuario = COALESCE(current_setting('app.current_user', true), current_user),
        row_version = row_version + 1
    WHERE id_vehiculo = NEW.id_vehiculo;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_post_checkin_checkout()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_id_vehiculo INT;
BEGIN
    SELECT id_vehiculo INTO v_id_vehiculo
    FROM rental.CONTRATOS
    WHERE id_contrato = NEW.id_contrato;

    IF NEW.tipo_check = 'CHECKOUT' THEN
        UPDATE rental.VEHICULOS
        SET estado_operativo = 'ALQUILADO',
            kilometraje_actual = GREATEST(kilometraje_actual, NEW.kilometraje),
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_vehiculo = v_id_vehiculo;
    ELSIF NEW.tipo_check = 'CHECKIN' THEN
        UPDATE rental.VEHICULOS
        SET estado_operativo = 'DISPONIBLE',
            kilometraje_actual = GREATEST(kilometraje_actual, NEW.kilometraje),
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_vehiculo = v_id_vehiculo;

        UPDATE rental.CONTRATOS
        SET estado_contrato = 'CERRADO',
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_contrato = NEW.id_contrato;

        UPDATE rental.RESERVAS
        SET estado_reserva = 'FINALIZADA',
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_reserva = (
            SELECT id_reserva FROM rental.CONTRATOS WHERE id_contrato = NEW.id_contrato
        );
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_post_mantenimiento_estado_vehiculo()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.estado_mantenimiento = 'ABIERTO' THEN
        UPDATE rental.VEHICULOS
        SET estado_operativo = CASE WHEN NEW.tipo_mantenimiento = 'TALLER' THEN 'TALLER' ELSE 'MANTENIMIENTO' END,
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_vehiculo = NEW.id_vehiculo;
    ELSIF NEW.estado_mantenimiento = 'CERRADO' THEN
        UPDATE rental.VEHICULOS
        SET estado_operativo = 'DISPONIBLE',
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = NEW.creado_por_usuario,
            row_version = row_version + 1
        WHERE id_vehiculo = NEW.id_vehiculo;
    END IF;

    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_validar_pago()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.id_reserva IS NULL AND NEW.id_contrato IS NULL THEN
        RAISE EXCEPTION 'Todo pago debe referenciar una reserva o un contrato';
    END IF;

    IF NEW.id_contrato IS NOT NULL AND NEW.id_reserva IS NULL THEN
        SELECT id_reserva INTO NEW.id_reserva
        FROM rental.CONTRATOS
        WHERE id_contrato = NEW.id_contrato;
    END IF;

    RETURN NEW;
END;
$$;

-- ----------------------------------------------------------
-- 3) PROCEDIMIENTOS / FUNCIONES DE USO TRANSACCIONAL
-- ----------------------------------------------------------

CREATE OR REPLACE PROCEDURE rental.sp_confirmar_reserva(
    IN p_id_reserva INT,
    IN p_usuario VARCHAR(100),
    IN p_ip VARCHAR(45) DEFAULT NULL,
    IN p_origen VARCHAR(20) DEFAULT 'API'
)
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM set_config('app.current_user', p_usuario, true);
    PERFORM set_config('app.current_ip', COALESCE(p_ip, ''), true);
    PERFORM set_config('app.current_origin', p_origen, true);

    UPDATE rental.RESERVAS
    SET estado_reserva = 'CONFIRMADA',
        modificado_por_usuario = p_usuario,
        modificado_desde_ip = p_ip,
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        row_version = row_version + 1
    WHERE id_reserva = p_id_reserva
      AND estado_reserva = 'PENDIENTE';

    IF NOT FOUND THEN
        RAISE EXCEPTION 'No se pudo confirmar la reserva %. Verifique que exista y esté en estado PENDIENTE', p_id_reserva;
    END IF;
END;
$$;

CREATE OR REPLACE PROCEDURE rental.sp_cancelar_reserva(
    IN p_id_reserva INT,
    IN p_motivo VARCHAR(250),
    IN p_usuario VARCHAR(100),
    IN p_ip VARCHAR(45) DEFAULT NULL,
    IN p_origen VARCHAR(20) DEFAULT 'API'
)
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM set_config('app.current_user', p_usuario, true);
    PERFORM set_config('app.current_ip', COALESCE(p_ip, ''), true);
    PERFORM set_config('app.current_origin', p_origen, true);

    UPDATE rental.RESERVAS
    SET estado_reserva = 'CANCELADA',
        fecha_cancelacion_utc = CURRENT_TIMESTAMP(0),
        motivo_cancelacion = p_motivo,
        modificado_por_usuario = p_usuario,
        modificado_desde_ip = p_ip,
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        row_version = row_version + 1
    WHERE id_reserva = p_id_reserva
      AND estado_reserva IN ('PENDIENTE', 'CONFIRMADA');

    IF NOT FOUND THEN
        RAISE EXCEPTION 'No se pudo cancelar la reserva %. Estado no permitido.', p_id_reserva;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION rental.fn_generar_contrato(
    p_id_reserva INT,
    p_usuario VARCHAR(100),
    p_kilometraje_salida INT,
    p_nivel_combustible_salida NUMERIC(5,2),
    p_pdf_url VARCHAR(300) DEFAULT NULL,
    p_observaciones VARCHAR(300) DEFAULT NULL,
    p_origen VARCHAR(20) DEFAULT 'API'
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_reserva rental.RESERVAS%ROWTYPE;
    v_id_contrato INT;
BEGIN
    PERFORM set_config('app.current_user', p_usuario, true);
    PERFORM set_config('app.current_origin', p_origen, true);

    SELECT * INTO v_reserva
    FROM rental.RESERVAS
    WHERE id_reserva = p_id_reserva;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'La reserva % no existe', p_id_reserva;
    END IF;

    IF v_reserva.estado_reserva NOT IN ('CONFIRMADA', 'EN_CURSO') THEN
        RAISE EXCEPTION 'La reserva % debe estar confirmada para generar contrato', p_id_reserva;
    END IF;

    INSERT INTO rental.CONTRATOS (
        numero_contrato, id_reserva, id_cliente, id_vehiculo,
        fecha_hora_salida, fecha_hora_prevista_devolucion,
        kilometraje_salida, nivel_combustible_salida,
        estado_contrato, pdf_url, observaciones_contrato,
        fecha_registro_utc, creado_por_usuario, origen_registro
    )
    VALUES (
        'CTR-' || TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS') || '-' || LPAD(p_id_reserva::TEXT, 6, '0'),
        v_reserva.id_reserva,
        v_reserva.id_cliente,
        v_reserva.id_vehiculo,
        v_reserva.fecha_hora_recogida,
        v_reserva.fecha_hora_devolucion,
        p_kilometraje_salida,
        p_nivel_combustible_salida,
        'ABIERTO',
        p_pdf_url,
        p_observaciones,
        CURRENT_TIMESTAMP(0),
        p_usuario,
        p_origen
    )
    RETURNING id_contrato INTO v_id_contrato;

    RETURN v_id_contrato;
END;
$$;

CREATE OR REPLACE PROCEDURE security.sp_registrar_login_exitoso(
    IN p_id_usuario INT,
    IN p_token_id VARCHAR(120),
    IN p_refresh_token_hash VARCHAR(500),
    IN p_fecha_expiracion_utc TIMESTAMPTZ,
    IN p_ip VARCHAR(45) DEFAULT NULL,
    IN p_user_agent VARCHAR(300) DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_username VARCHAR(100);
BEGIN
    UPDATE security.USUARIOS_APP
    SET ultimo_login_utc = CURRENT_TIMESTAMP(0),
        intentos_fallidos = 0,
        bloqueado_hasta_utc = NULL,
        fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
        modificado_por_usuario = 'LOGIN_OK',
        row_version = row_version + 1
    WHERE id_usuario = p_id_usuario;

    INSERT INTO security.SESIONES (
        id_usuario, token_id, refresh_token_hash, ip_origen, user_agent,
        fecha_inicio_utc, fecha_expiracion_utc, estado_sesion,
        creado_por_usuario
    )
    VALUES (
        p_id_usuario, p_token_id, p_refresh_token_hash, p_ip, p_user_agent,
        CURRENT_TIMESTAMP(0), p_fecha_expiracion_utc, 'ACTIVA', 'LOGIN_OK'
    );

    SELECT username INTO v_username
    FROM security.USUARIOS_APP
    WHERE id_usuario = p_id_usuario;

    INSERT INTO audit.AUD_EVENTOS (
        esquema_afectado, tabla_afectada, operacion, id_registro_afectado,
        datos_anteriores, datos_nuevos, usuario_app, login_bd, ip_origen,
        origen_evento, fecha_evento_utc, row_version
    )
    VALUES (
        'security', 'USUARIOS_APP', 'LOGIN', p_id_usuario::TEXT,
        NULL, jsonb_build_object('token_id', p_token_id, 'estado', 'ACTIVA')::TEXT,
        v_username, current_user, p_ip,
        'API', CURRENT_TIMESTAMP(0), 1
    );
END;
$$;

CREATE OR REPLACE PROCEDURE security.sp_registrar_login_fallido(
    IN p_username VARCHAR(50),
    IN p_ip VARCHAR(45) DEFAULT NULL,
    IN p_motivo VARCHAR(250) DEFAULT 'Credenciales inválidas'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_id_usuario INT;
    v_intentos SMALLINT;
BEGIN
    SELECT id_usuario, intentos_fallidos
    INTO v_id_usuario, v_intentos
    FROM security.USUARIOS_APP
    WHERE username = p_username;

    IF FOUND THEN
        UPDATE security.USUARIOS_APP
        SET intentos_fallidos = intentos_fallidos + 1,
            bloqueado_hasta_utc = CASE WHEN intentos_fallidos + 1 >= 5 THEN CURRENT_TIMESTAMP(0) + INTERVAL '15 minutes' ELSE bloqueado_hasta_utc END,
            estado_usuario = CASE WHEN intentos_fallidos + 1 >= 5 THEN 'BLQ' ELSE estado_usuario END,
            fecha_modificacion_utc = CURRENT_TIMESTAMP(0),
            modificado_por_usuario = 'LOGIN_FAIL',
            row_version = row_version + 1
        WHERE id_usuario = v_id_usuario;
    END IF;

    INSERT INTO audit.AUD_INTENTOS_LOGIN (
        username_intentado, resultado, motivo, ip_origen, user_agent,
        fecha_evento_utc, row_version
    )
    VALUES (
        p_username, 'FALLIDO', p_motivo, p_ip, NULL,
        CURRENT_TIMESTAMP(0), 1
    );

    INSERT INTO audit.AUD_EVENTOS (
        esquema_afectado, tabla_afectada, operacion, id_registro_afectado,
        datos_anteriores, datos_nuevos, usuario_app, login_bd, ip_origen,
        origen_evento, fecha_evento_utc, row_version
    )
    VALUES (
        'security', 'USUARIOS_APP', 'FAILED_LOGIN', COALESCE(v_id_usuario::TEXT, p_username),
        NULL, jsonb_build_object('motivo', p_motivo)::TEXT,
        p_username, current_user, p_ip,
        'API', CURRENT_TIMESTAMP(0), 1
    );
END;
$$;

-- ----------------------------------------------------------
-- 4) TRIGGERS
-- ----------------------------------------------------------

-- Timestamps + row version
DROP TRIGGER IF EXISTS trg_touch_vehiculos ON rental.VEHICULOS;
CREATE TRIGGER trg_touch_vehiculos
BEFORE UPDATE ON rental.VEHICULOS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_clientes ON rental.CLIENTES;
CREATE TRIGGER trg_touch_clientes
BEFORE UPDATE ON rental.CLIENTES
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_conductores ON rental.CONDUCTORES;
CREATE TRIGGER trg_touch_conductores
BEFORE UPDATE ON rental.CONDUCTORES
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_reservas ON rental.RESERVAS;
CREATE TRIGGER trg_touch_reservas
BEFORE UPDATE ON rental.RESERVAS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_reserva_extras ON rental.RESERVA_EXTRAS;
CREATE TRIGGER trg_touch_reserva_extras
BEFORE UPDATE ON rental.RESERVA_EXTRAS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_contratos ON rental.CONTRATOS;
CREATE TRIGGER trg_touch_contratos
BEFORE UPDATE ON rental.CONTRATOS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_pagos ON rental.PAGOS;
CREATE TRIGGER trg_touch_pagos
BEFORE UPDATE ON rental.PAGOS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_facturas ON rental.FACTURAS;
CREATE TRIGGER trg_touch_facturas
BEFORE UPDATE ON rental.FACTURAS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_mantenimientos ON rental.MANTENIMIENTOS;
CREATE TRIGGER trg_touch_mantenimientos
BEFORE UPDATE ON rental.MANTENIMIENTOS
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_usuarios_app ON security.USUARIOS_APP;
CREATE TRIGGER trg_touch_usuarios_app
BEFORE UPDATE ON security.USUARIOS_APP
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_sesiones ON security.SESIONES;
CREATE TRIGGER trg_touch_sesiones
BEFORE UPDATE ON security.SESIONES
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_api_clientes ON security.API_CLIENTES;
CREATE TRIGGER trg_touch_api_clientes
BEFORE UPDATE ON security.API_CLIENTES
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

DROP TRIGGER IF EXISTS trg_touch_localizacion_extra_stock ON rental.LOCALIZACION_EXTRA_STOCK;
CREATE TRIGGER trg_touch_localizacion_extra_stock
BEFORE UPDATE ON rental.LOCALIZACION_EXTRA_STOCK
FOR EACH ROW EXECUTE FUNCTION public.fn_touch_audit_fields();

-- Validaciones y reglas de negocio
DROP TRIGGER IF EXISTS trg_validar_reserva ON rental.RESERVAS;
CREATE TRIGGER trg_validar_reserva
BEFORE INSERT OR UPDATE ON rental.RESERVAS
FOR EACH ROW EXECUTE FUNCTION rental.fn_validar_reserva();

DROP TRIGGER IF EXISTS trg_validar_stock_extra ON rental.RESERVA_EXTRAS;
CREATE TRIGGER trg_validar_stock_extra
BEFORE INSERT OR UPDATE ON rental.RESERVA_EXTRAS
FOR EACH ROW EXECUTE FUNCTION rental.fn_validar_stock_extra();

DROP TRIGGER IF EXISTS trg_actualizar_stock_extra ON rental.RESERVA_EXTRAS;
CREATE TRIGGER trg_actualizar_stock_extra
AFTER INSERT OR UPDATE OR DELETE ON rental.RESERVA_EXTRAS
FOR EACH ROW EXECUTE FUNCTION rental.fn_actualizar_stock_extra();

DROP TRIGGER IF EXISTS trg_sync_reserva_extras_total ON rental.RESERVA_EXTRAS;
CREATE TRIGGER trg_sync_reserva_extras_total
AFTER INSERT OR UPDATE OR DELETE ON rental.RESERVA_EXTRAS
FOR EACH ROW EXECUTE FUNCTION rental.fn_sync_reserva_extras_total();

DROP TRIGGER IF EXISTS trg_validar_contrato ON rental.CONTRATOS;
CREATE TRIGGER trg_validar_contrato
BEFORE INSERT OR UPDATE ON rental.CONTRATOS
FOR EACH ROW EXECUTE FUNCTION rental.fn_validar_contrato();

DROP TRIGGER IF EXISTS trg_post_contrato_creado ON rental.CONTRATOS;
CREATE TRIGGER trg_post_contrato_creado
AFTER INSERT ON rental.CONTRATOS
FOR EACH ROW EXECUTE FUNCTION rental.fn_post_contrato_creado();

DROP TRIGGER IF EXISTS trg_post_checkin_checkout ON rental.CHECKIN_OUT;
CREATE TRIGGER trg_post_checkin_checkout
AFTER INSERT ON rental.CHECKIN_OUT
FOR EACH ROW EXECUTE FUNCTION rental.fn_post_checkin_checkout();

DROP TRIGGER IF EXISTS trg_post_mantenimiento_estado_vehiculo ON rental.MANTENIMIENTOS;
CREATE TRIGGER trg_post_mantenimiento_estado_vehiculo
AFTER INSERT OR UPDATE ON rental.MANTENIMIENTOS
FOR EACH ROW EXECUTE FUNCTION rental.fn_post_mantenimiento_estado_vehiculo();

DROP TRIGGER IF EXISTS trg_validar_pago ON rental.PAGOS;
CREATE TRIGGER trg_validar_pago
BEFORE INSERT OR UPDATE ON rental.PAGOS
FOR EACH ROW EXECUTE FUNCTION rental.fn_validar_pago();

-- Auditoría de cambios relevantes
DROP TRIGGER IF EXISTS trg_aud_reservas ON rental.RESERVAS;
CREATE TRIGGER trg_aud_reservas
AFTER INSERT OR UPDATE OR DELETE ON rental.RESERVAS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_reserva_extras ON rental.RESERVA_EXTRAS;
CREATE TRIGGER trg_aud_reserva_extras
AFTER INSERT OR UPDATE OR DELETE ON rental.RESERVA_EXTRAS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_contratos ON rental.CONTRATOS;
CREATE TRIGGER trg_aud_contratos
AFTER INSERT OR UPDATE OR DELETE ON rental.CONTRATOS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_checkin_out ON rental.CHECKIN_OUT;
CREATE TRIGGER trg_aud_checkin_out
AFTER INSERT OR UPDATE OR DELETE ON rental.CHECKIN_OUT
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_pagos ON rental.PAGOS;
CREATE TRIGGER trg_aud_pagos
AFTER INSERT OR UPDATE OR DELETE ON rental.PAGOS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_facturas ON rental.FACTURAS;
CREATE TRIGGER trg_aud_facturas
AFTER INSERT OR UPDATE OR DELETE ON rental.FACTURAS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_vehiculos ON rental.VEHICULOS;
CREATE TRIGGER trg_aud_vehiculos
AFTER INSERT OR UPDATE OR DELETE ON rental.VEHICULOS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_clientes ON rental.CLIENTES;
CREATE TRIGGER trg_aud_clientes
AFTER INSERT OR UPDATE OR DELETE ON rental.CLIENTES
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_conductores ON rental.CONDUCTORES;
CREATE TRIGGER trg_aud_conductores
AFTER INSERT OR UPDATE OR DELETE ON rental.CONDUCTORES
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_mantenimientos ON rental.MANTENIMIENTOS;
CREATE TRIGGER trg_aud_mantenimientos
AFTER INSERT OR UPDATE OR DELETE ON rental.MANTENIMIENTOS
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_usuarios_app ON security.USUARIOS_APP;
CREATE TRIGGER trg_aud_usuarios_app
AFTER INSERT OR UPDATE OR DELETE ON security.USUARIOS_APP
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_sesiones ON security.SESIONES;
CREATE TRIGGER trg_aud_sesiones
AFTER INSERT OR UPDATE OR DELETE ON security.SESIONES
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

DROP TRIGGER IF EXISTS trg_aud_api_clientes ON security.API_CLIENTES;
CREATE TRIGGER trg_aud_api_clientes
AFTER INSERT OR UPDATE OR DELETE ON security.API_CLIENTES
FOR EACH ROW EXECUTE FUNCTION audit.fn_log_row_change();

