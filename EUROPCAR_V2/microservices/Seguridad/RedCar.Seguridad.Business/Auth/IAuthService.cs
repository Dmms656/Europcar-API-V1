namespace RedCar.Seguridad.Business.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<object> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
