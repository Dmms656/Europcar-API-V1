using Npgsql;

// No incluir credenciales en el repositorio. Usar la misma cadena que la API, por ejemplo:
//   $env:ConnectionStrings__RentalDb = "Host=...;Password=...;..."
// o   set ConnectionStrings__RentalDb=...
var cs = Environment.GetEnvironmentVariable("ConnectionStrings__RentalDb")
    ?? Environment.GetEnvironmentVariable("RENTAL_DB_CONNECTION")
    ?? throw new InvalidOperationException(
        "Define ConnectionStrings__RentalDb o RENTAL_DB_CONNECTION con la cadena Npgsql.");

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
