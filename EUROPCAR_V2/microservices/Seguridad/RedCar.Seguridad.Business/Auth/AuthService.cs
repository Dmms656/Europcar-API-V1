using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using NpgsqlTypes;
using RedCar.Seguridad.DataAccess.Context;
using RedCar.Shared.Auth;

namespace RedCar.Seguridad.Business.Auth;

public sealed class AuthService : IAuthService
{
    private readonly SeguridadDbContext _db;
    private readonly IOptions<JwtSettings> _jwt;

    public AuthService(SeguridadDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (_db.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            throw new UnauthorizedAccessException("Login requiere PostgreSQL con extension pgcrypto y seed security.");

        var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // NpgsqlCommand + tipos explicitos: evita rarezas de DbCommand generico con el pooler.
        await using var cmd = new NpgsqlCommand(
            """
            SELECT u.id_usuario, u.username, u.correo, u.id_cliente, u.bloqueado_hasta_utc,
                   COALESCE(string_agg(DISTINCT r.nombre_rol, ',' ORDER BY r.nombre_rol), '') AS roles_csv
              FROM security.usuarios_app u
              LEFT JOIN security.usuarios_roles ur ON ur.id_usuario = u.id_usuario
                    AND ur.estado_usuario_rol = 'ACT' AND ur.activo AND NOT ur.es_eliminado
              LEFT JOIN security.roles r ON r.id_rol = ur.id_rol AND r.estado_rol = 'ACT'
             WHERE lower(u.username) = lower(@username)
               AND u.activo AND NOT u.es_eliminado AND u.estado_usuario = 'ACT'
               AND crypt(@password, u.password_hash) = u.password_hash
             GROUP BY u.id_usuario, u.username, u.correo, u.id_cliente, u.bloqueado_hasta_utc
            """,
            conn);
        cmd.Parameters.Add(new NpgsqlParameter("username", NpgsqlDbType.Text) { Value = request.Username.Trim() });
        cmd.Parameters.Add(new NpgsqlParameter("password", NpgsqlDbType.Text) { Value = request.Password ?? string.Empty });

        await using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (!await rdr.ReadAsync(ct))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var ordB = rdr.GetOrdinal("bloqueado_hasta_utc");
        DateTime? bloqueado = rdr.IsDBNull(ordB) ? null : rdr.GetDateTime(ordB);
        if (bloqueado.HasValue && bloqueado.Value > DateTime.UtcNow)
            throw new UnauthorizedAccessException($"Usuario bloqueado hasta {bloqueado:yyyy-MM-dd HH:mm:ss} UTC");

        var idUsuario = rdr.GetInt32(rdr.GetOrdinal("id_usuario"));
        var username = rdr.GetString(rdr.GetOrdinal("username"));
        var correo = rdr.GetString(rdr.GetOrdinal("correo"));
        var idCliente = rdr.IsDBNull(rdr.GetOrdinal("id_cliente")) ? (int?)null : rdr.GetInt32(rdr.GetOrdinal("id_cliente"));
        var rolesCsv = rdr.IsDBNull(rdr.GetOrdinal("roles_csv")) ? "" : rdr.GetString(rdr.GetOrdinal("roles_csv"));
        await rdr.CloseAsync();

        var roleList = rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roleList.Any(r => string.Equals(r, "AGENTE", StringComparison.OrdinalIgnoreCase))
            && !roleList.Any(r => string.Equals(r, "AGENTE_POS", StringComparison.OrdinalIgnoreCase)))
        {
            roleList.Add("AGENTE_POS");
        }

        var (token, exp) = CreateJwt(idUsuario, username, correo, roleList, idCliente);

        return new LoginResponseDto
        {
            Token = token,
            Username = username,
            Correo = correo,
            Roles = roleList,
            Expiration = exp,
            IdCliente = idCliente,
            NombreCompleto = null
        };
    }

    public async Task<object> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (_db.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            throw new InvalidOperationException("Registro requiere PostgreSQL.");

        var user = request.Username.Trim();
        var mail = request.Correo.Trim().ToLowerInvariant();

        var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            await using (var exists = conn.CreateCommand())
            {
                exists.Transaction = tx;
                exists.CommandText = "SELECT 1 FROM security.usuarios_app WHERE lower(username)=lower(@u) OR lower(correo)=lower(@m) LIMIT 1";
                var e1 = exists.CreateParameter();
                e1.ParameterName = "u";
                e1.Value = user;
                exists.Parameters.Add(e1);
                var e2 = exists.CreateParameter();
                e2.ParameterName = "m";
                e2.Value = mail;
                exists.Parameters.Add(e2);
                if (await exists.ExecuteScalarAsync(ct) is not null)
                    throw new InvalidOperationException("El nombre de usuario o el correo ya están en uso");
            }

            int newId;
            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = """
                    INSERT INTO security.usuarios_app (
                        username, correo, password_hash, password_salt, password_hint,
                        requiere_cambio_password, estado_usuario, es_eliminado, activo,
                        intentos_fallidos, id_cliente, cliente_guid, fecha_registro_utc, creado_por_usuario
                    )
                    SELECT @user, @mail, crypt(@pwd, s.salt), s.salt, 'registro_web',
                           FALSE, 'ACT', FALSE, TRUE, 0, @idCliente, NULL,
                           CURRENT_TIMESTAMP(0), 'api_register'
                    FROM (SELECT gen_salt('bf') AS salt) s
                    RETURNING id_usuario
                    """;
                var pUser = ins.CreateParameter();
                pUser.ParameterName = "user";
                pUser.Value = user;
                ins.Parameters.Add(pUser);
                var pMail = ins.CreateParameter();
                pMail.ParameterName = "mail";
                pMail.Value = mail;
                ins.Parameters.Add(pMail);
                var pPwd = ins.CreateParameter();
                pPwd.ParameterName = "pwd";
                pPwd.Value = request.Password;
                ins.Parameters.Add(pPwd);
                var pIdCliente = ins.CreateParameter();
                pIdCliente.ParameterName = "idCliente";
                pIdCliente.Value = (object?)request.IdCliente ?? DBNull.Value;
                ins.Parameters.Add(pIdCliente);

                var newIdObj = await ins.ExecuteScalarAsync(ct)
                             ?? throw new InvalidOperationException("No se pudo crear el usuario");
                newId = Convert.ToInt32(newIdObj);
            }

            await using (var rolCmd = conn.CreateCommand())
            {
                rolCmd.Transaction = tx;
                rolCmd.CommandText = """
                    INSERT INTO security.usuarios_roles (id_usuario, id_rol, estado_usuario_rol, es_eliminado, activo, creado_por_usuario)
                    SELECT @id, id_rol, 'ACT', FALSE, TRUE, 'api_register'
                      FROM security.roles WHERE nombre_rol = 'CLIENTE_WEB' LIMIT 1
                    """;
                var pId = rolCmd.CreateParameter();
                pId.ParameterName = "id";
                pId.Value = newId;
                rolCmd.Parameters.Add(pId);
                await rolCmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return new { userId = newId, username = user, idCliente = request.IdCliente, message = "Usuario registrado exitosamente" };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private (string Token, DateTime Expiration) CreateJwt(int userId, string username, string correo, IReadOnlyList<string> roleNames, int? idCliente)
    {
        var s = _jwt.Value;
        if (string.IsNullOrWhiteSpace(s.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey no configurado");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, correo),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (idCliente.HasValue)
            claims.Add(new Claim("idCliente", idCliente.Value.ToString()));
        foreach (var rn in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, rn));

        var exp = DateTime.UtcNow.AddMinutes(s.ExpirationMinutes <= 0 ? 60 : s.ExpirationMinutes);
        var token = new JwtSecurityToken(
            issuer: s.Issuer,
            audience: s.Audience,
            claims: claims,
            expires: exp,
            signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), exp);
    }
}
