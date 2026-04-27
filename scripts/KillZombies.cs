using Npgsql;

var cs = "Host=db.ufqzdzdkcqmwvapdaajx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=eNbvTDIa2YJXRn1h;Ssl Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=15";

try
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    Console.WriteLine("Connected");

    // 1. Check zombie connections
    await using var cmdZ = new NpgsqlCommand(@"
        SELECT pid, state, NOW() - query_start AS duration, LEFT(query, 120) as query
        FROM pg_stat_activity
        WHERE state IN ('idle in transaction', 'active')
          AND query_start < NOW() - INTERVAL '10 seconds'
          AND pid <> pg_backend_pid()
        ORDER BY query_start", conn);
    await using var reader = await cmdZ.ExecuteReaderAsync();
    int count = 0;
    while (await reader.ReadAsync())
    {
        count++;
        Console.WriteLine($"ZOMBIE PID:{reader["pid"]} State:{reader["state"]} Duration:{reader["duration"]} Query:{reader["query"]}");
    }
    Console.WriteLine($"Total zombies: {count}");
    await reader.CloseAsync();

    // 2. Kill ALL zombies
    if (count > 0)
    {
        await using var cmdKill = new NpgsqlCommand(@"
            SELECT count(*) FROM (
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE pid <> pg_backend_pid()
                  AND (state = 'idle in transaction' OR (state = 'active' AND query_start < NOW() - INTERVAL '10 seconds'))
            ) t", conn);
        var killed = await cmdKill.ExecuteScalarAsync();
        Console.WriteLine($"Killed: {killed}");
    }

    // 3. Cleanup test data
    await using var cmdClean = new NpgsqlCommand("DELETE FROM rental.pagos WHERE codigo_pago = 'PAG-DIRECTTEST'", conn);
    var deleted = await cmdClean.ExecuteNonQueryAsync();
    Console.WriteLine($"Cleaned test rows: {deleted}");

    // 4. Check locks on pagos/reservas tables
    await using var cmdLocks = new NpgsqlCommand(@"
        SELECT l.pid, l.locktype, l.mode, l.granted, c.relname
        FROM pg_locks l
        JOIN pg_class c ON l.relation = c.oid
        WHERE c.relname IN ('pagos', 'reservas', 'facturas')
        ORDER BY c.relname, l.pid", conn);
    await using var lockReader = await cmdLocks.ExecuteReaderAsync();
    Console.WriteLine("\n=== Active locks on pagos/reservas/facturas ===");
    int lockCount = 0;
    while (await lockReader.ReadAsync())
    {
        lockCount++;
        Console.WriteLine($"PID:{lockReader["pid"]} Table:{lockReader["relname"]} Type:{lockReader["locktype"]} Mode:{lockReader["mode"]} Granted:{lockReader["granted"]}");
    }
    Console.WriteLine($"Total locks: {lockCount}");

    Console.WriteLine("\nDone!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
