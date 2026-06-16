using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Options;
using Telemetry.Contracts;
using Telemetry.Infrastructure.Configuration;
using ValidationResult = FluentValidation.Results.ValidationResult;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        RabbitMqOptions rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        
        cfg.Host(rabbitMqOptions.Host, "/", h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });
    });
});

WebApplication app = builder.Build();

app.MapPost("/readings", async (
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
        Timestamp = request.Timestamp,
    };

    await publishEndpoint.Publish(message, ctx =>
    {
        ctx.MessageId = correlationId;
        ctx.CorrelationId = correlationId;
    }, cancellationToken);

    return Results.Accepted(value: new { messageId = correlationId });
});

app.Run();
