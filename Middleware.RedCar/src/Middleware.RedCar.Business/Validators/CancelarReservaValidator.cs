using FluentValidation;
using Middleware.RedCar.Business.DTOs.Reservas;

namespace Middleware.RedCar.Business.Validators;

public sealed class CancelarReservaValidator : AbstractValidator<CancelarReservaRequest>
{
    public CancelarReservaValidator()
    {
        RuleFor(r => r.MotivoCancelacion)
            .NotEmpty()
            .WithMessage("motivoCancelacion es obligatorio para cancelar una reserva.")
            .MaximumLength(300)
            .WithMessage("motivoCancelacion no puede superar 300 caracteres.");
    }
}
