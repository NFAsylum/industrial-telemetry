using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Telemetry.Infrastructure.Configuration;
using Telemetry.Ingestion.Api.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    RedisOptions redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
    return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
});

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

EndpointsRegister.RegisterEndpoints(app);

app.Run();
