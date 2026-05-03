using MassTransit;
using Microsoft.Extensions.Options;
using Telemetry.Contracts;
using Telemetry.Infrastructure.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

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

app.MapPost("/readings", async (IngestionRequest request, IPublishEndpoint publishEndpoint) =>
{
    SensorReadingMessage message = new SensorReadingMessage()
    {
        MessageId = Guid.NewGuid(),
        SensorId = request.SensorId,
        Value = request.Value,
        Unit = request.Unit,
        Timestamp = request.Timestamp,
    };

    await publishEndpoint.Publish(message);

    return Results.Accepted(null, new { message.MessageId });
});

app.Run();
