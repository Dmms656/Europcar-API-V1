namespace RedCar.Shared.Contracts.Common;

/// <summary>
/// Codigos de error estables que viajan en ApiError.Code.
/// Si vas a anadir uno, mantenlo unico y documentalo.
/// </summary>
public static class ErrorCodes
{
    public const string Unknown            = "RC-0000";
    public const string Validation         = "RC-1000";
    public const string NotFound           = "RC-1001";
    public const string Conflict           = "RC-1002";
    public const string Forbidden          = "RC-1003";
    public const string Unauthorized       = "RC-1004";

    public const string DatabaseError      = "RC-2000";
    public const string ExternalServiceError = "RC-2001";

    public const string InvalidCredentials = "RC-3000";
    public const string TokenExpired       = "RC-3001";
    public const string UserLocked         = "RC-3002";
}
