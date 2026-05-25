using FluentValidation;
using Middleware.RedCar.Business.DTOs.Reservas;

namespace Middleware.RedCar.Business.Validators;

/// <summary>
/// Validacion sintactica y semantica del body del Endpoint 8 (POST crear reserva)
/// segun las reglas del contrato.
/// </summary>
public sealed class CrearReservaValidator : AbstractValidator<CrearReservaBookingRequest>
{
    private static readonly string[] TiposIdentificacion = { "CEDULA", "PASAPORTE", "RUC" };

    public CrearReservaValidator()
    {
        RuleFor(r => r.IdVehiculo).GreaterThan(0);
        RuleFor(r => r.IdLocalizacionRecogida).GreaterThan(0);
        RuleFor(r => r.IdLocalizacionDevolucion).GreaterThan(0);

        RuleFor(r => r.FechaInicio)
            .Must(f => f != default)
            .WithMessage("fechaInicio es obligatoria (YYYY-MM-DD).");

        RuleFor(r => r.FechaFin)
            .Must(f => f != default)
            .WithMessage("fechaFin es obligatoria (YYYY-MM-DD).");

        RuleFor(r => r)
            .Must(r => r.FechaFin >= r.FechaInicio)
            .WithMessage("fechaFin no puede ser anterior a fechaInicio.")
            .OverridePropertyName("fechaFin");

        RuleFor(r => r)
            .Must(r => r.FechaFin.ToDateTime(r.HoraFin) > r.FechaInicio.ToDateTime(r.HoraInicio))
            .WithMessage("La fecha y hora de devolucion deben ser posteriores a la de inicio.")
            .OverridePropertyName("horaFin");

        RuleFor(r => r.Observaciones)
            .MaximumLength(300).When(r => r.Observaciones is not null)
            .WithMessage("observaciones no puede superar 300 caracteres.");

        RuleFor(r => r.Cliente).NotNull();
        When(r => r.Cliente is not null, () =>
        {
            RuleFor(r => r.Cliente.Nombres).NotEmpty().MaximumLength(160);
            RuleFor(r => r.Cliente.Apellidos).NotEmpty().MaximumLength(160);
            RuleFor(r => r.Cliente.TipoIdentificacion)
                .NotEmpty()
                .Must(t => TiposIdentificacion.Contains(t))
                .WithMessage("tipoIdentificacion debe ser CEDULA, PASAPORTE o RUC.");
            RuleFor(r => r.Cliente.NumeroIdentificacion).NotEmpty().MaximumLength(40);
            RuleFor(r => r.Cliente.Correo).NotEmpty().EmailAddress();
            RuleFor(r => r.Cliente.Telefono).NotEmpty().MaximumLength(30);
        });

        RuleFor(r => r.Conductores)
            .NotEmpty().WithMessage("Debe enviarse al menos un conductor.");

        RuleFor(r => r.Conductores)
            .Must(cs => cs is null || cs.Count(c => c.EsPrincipal) == 1)
            .WithMessage("Debe haber exactamente un conductor con esPrincipal = true.")
            .When(r => r.Conductores is not null && r.Conductores.Count > 0);

        RuleForEach(r => r.Conductores).ChildRules(c =>
        {
            c.RuleFor(x => x.Nombres).NotEmpty().MaximumLength(160);
            c.RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(160);
            c.RuleFor(x => x.TipoIdentificacion)
                .NotEmpty()
                .Must(t => TiposIdentificacion.Contains(t));
            c.RuleFor(x => x.NumeroIdentificacion).NotEmpty().MaximumLength(40);
            c.RuleFor(x => x.FechaVencimientoLicencia)
                .Must(f => f > DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("La licencia de conducir esta vencida.");
            c.RuleFor(x => x.EdadConductor).InclusiveBetween(18, 100);
            c.RuleFor(x => x.Correo).NotEmpty().EmailAddress();
            c.RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        });

        RuleForEach(r => r.Extras).ChildRules(e =>
        {
            e.RuleFor(x => x.IdExtra).GreaterThan(0);
            e.RuleFor(x => x.Cantidad).InclusiveBetween(1, 99);
        });
    }
}
