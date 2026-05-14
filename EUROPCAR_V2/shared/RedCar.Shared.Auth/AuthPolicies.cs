namespace RedCar.Shared.Auth;

/// <summary>
/// Constantes de policies y roles compartidos para [Authorize(Roles = ...)] / [Authorize(Policy = ...)].
/// Mantener centralizado evita tipos magicos repetidos en cada controller.
/// </summary>
public static class AuthPolicies
{
    public static class Roles
    {
        public const string Admin     = "ADMIN";
        public const string AgentePos = "AGENTE_POS";
        public const string ClienteWeb = "CLIENTE_WEB";
    }

    public static class Claims
    {
        public const string UsuarioGuid = "usuario_guid";
        public const string IdUsuario   = "id_usuario";
    }
}
