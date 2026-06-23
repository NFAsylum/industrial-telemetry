using FluentValidation;
using MassTransit;
using Telemetry.Contracts;
using Telemetry.Ingestion.Api.Filters;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Telemetry.Ingestion.Api.Endpoints;

public static class ReadingsEndpoint
{
    public static void Register(WebApplication app)
    {
        app.MapPost("/readings", async Task<object> (
            IngestionRequest request,
            IValidator<IngestionRequest> validator,
            IPublishEndpoint publishEndpoint,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            Guid correlationId = Guid.NewGuid();
            httpContext.Response.Headers["X-Correlation-Id"] = correlationId.ToString();

            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            SensorReadingMessage message = new SensorReadingMessage()
            {
                MessageId = correlationId,
                SensorId = request.SensorId,
                Value = request.Value,
                Unit = request.Unit,
                Timestamp = request.Timestamp
            };

            await publishEndpoint.Publish(message, ctx =>
            {
                ctx.MessageId = correlationId;
                ctx.CorrelationId = correlationId;
            }, cancellationToken);
    
            return Results.Accepted(value: new IngestionResponse { MessageId = correlationId });
        }).AddEndpointFilter<IdempotencyEndpointFilter>();
    }
}