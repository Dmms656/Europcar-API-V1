-- Vincula el usuario de login con el cliente Dylan Medina (cédula 1724820194).
-- Ejecutar una vez en Supabase si el usuario se creó desde /registro sin id_cliente.
-- Alternativa: en el front, pestaña «Cliente Existente» + cédula 1724820194 al registrarse de nuevo.

UPDATE security.usuarios_app u
SET
    id_cliente = c.id_cliente,
    cliente_guid = c.cliente_guid
FROM clientes.clientes c
WHERE lower(u.correo) = lower('medinadylan@gmail.com')
  AND c.numero_identificacion = '1724820194'
  AND NOT c.es_eliminado
  AND c.estado_cliente = 'ACT';
