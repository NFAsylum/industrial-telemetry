using FluentValidation;
using Telemetry.Contracts;

namespace Telemetry.Ingestion.Api;

public class IngestionRequestValidator : AbstractValidator<IngestionRequest>
{
    public IngestionRequestValidator()
    {
        RuleFor(request => request.SensorId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Unit)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Value)
            .Must(value => !double.IsNaN(value) && !double.IsInfinity(value))
            .WithMessage("Value must be a finite number.");
        
        RuleFor(request => request.Timestamp)
            .Must(timestamp => timestamp <= DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be more than 5 minutes in the future.");
    }
}