using Npgsql;

var cs = "Host=db.ufqzdzdkcqmwvapdaajx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=eNbvTDIa2YJXRn1h;Ssl Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=15";

try
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    Console.WriteLine("Connected");

    // Test INSERT factura with id_reserva=6
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await using var cmd = new NpgsqlCommand(@"
        INSERT INTO rental.facturas
        (factura_guid, numero_factura, id_cliente, id_reserva, fecha_emision,
         subtotal, valor_iva, total, estado_factura, servicio_origen, origen_canal_factura,
         observaciones_factura, creado_por_usuario, fecha_registro_utc)
        VALUES (gen_random_uuid(), 'FAC-DIRECT-TEST', 9, 6,
                CURRENT_TIMESTAMP, 73.91, 11.09, 85, 'EMITIDA', 'TEST', 'WEB',
                'Test directo', 'test', CURRENT_TIMESTAMP)
        RETURNING id_factura", conn);
    cmd.CommandTimeout = 10;
    var id = await cmd.ExecuteScalarAsync();
    Console.WriteLine($"Factura INSERT OK ({sw.ElapsedMilliseconds}ms) - id_factura: {id}");

    // Cleanup
    await using var cmdDel = new NpgsqlCommand("DELETE FROM rental.facturas WHERE numero_factura = 'FAC-DIRECT-TEST'", conn);
    await cmdDel.ExecuteNonQueryAsync();
    Console.WriteLine("Cleaned up");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Inner: {ex.InnerException?.Message}");
}
