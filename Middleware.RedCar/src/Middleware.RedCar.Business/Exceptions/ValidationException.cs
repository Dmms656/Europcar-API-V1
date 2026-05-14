namespace Middleware.RedCar.Business.Exceptions;

/// <summary>
/// 422 - Error de validacion de negocio. Lleva la lista de errores por campo
/// para que el middleware los exponga en la respuesta.
/// </summary>
public sealed class ValidationException : BusinessException
{
    public IReadOnlyList<ValidationFailure> Failures { get; }

    public ValidationException(IReadOnlyList<ValidationFailure> failures, string? message = null)
        : base(message ?? "Error de validacion de negocio.", 422)
    {
        Failures = failures;
    }
}

public sealed record ValidationFailure(string Field, string Message);
