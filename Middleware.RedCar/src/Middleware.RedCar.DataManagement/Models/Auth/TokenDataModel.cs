namespace Middleware.RedCar.DataManagement.Models.Auth;

public sealed record TokenDataModel(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    DateTimeOffset IssuedAtUtc);
