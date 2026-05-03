using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telemetry.Infrastructure;
using Telemetry.Infrastructure.Configuration;
using Telemetry.Worker;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.Configure<DatabaseOptions>(
        context.Configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RabbitMqOptions>(
            context.Configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddDbContext<TelemetryDbContext>((sp, options) =>
        {
            DatabaseOptions db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(db.ConnectionString);
        });

    services.AddMassTransit(x =>
    {
        x.AddConsumer<SensorReadingConsumer>();
        
        x.UsingRabbitMq((context, cfg) =>
        {
            RabbitMqOptions rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            
            cfg.Host(rabbitMqOptions.Host, "/", h =>
            {
                h.Username(rabbitMqOptions.Username);
                h.Password(rabbitMqOptions.Password);
            });
            
            cfg.ConfigureEndpoints(context);
        });   
    });
});

IHost host = builder.Build();

using (IServiceScope scope = host.Services.CreateScope())
{
    TelemetryDbContext db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    db.Database.Migrate();
}

host.Run();
