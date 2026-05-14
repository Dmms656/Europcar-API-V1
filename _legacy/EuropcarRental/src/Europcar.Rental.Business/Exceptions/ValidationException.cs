namespace Europcar.Rental.Business.Exceptions;

/// <summary>
/// Excepción de validación de negocio con errores por campo.
/// Se serializa como un diccionario { campo: [mensajes] }
/// y produce HTTP 422 (Unprocessable Entity).
/// </summary>
public class ValidationException : BusinessException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message, 422)
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this("Uno o más campos no son válidos", errors) { }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = new[] { error } }) { }
}
