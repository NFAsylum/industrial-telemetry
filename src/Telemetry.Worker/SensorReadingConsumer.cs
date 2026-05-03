using MassTransit;
using Telemetry.Contracts;
using Telemetry.Domain;
using Telemetry.Infrastructure;

namespace Telemetry.Worker;

public class SensorReadingConsumer : IConsumer<SensorReadingMessage>
{
    private readonly TelemetryDbContext _db;

    public SensorReadingConsumer(TelemetryDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<SensorReadingMessage> context)
    {
        SensorReadingMessage msg = context.Message;

        SensorReading reading = new SensorReading()
        {
            Id = Guid.NewGuid(),
            SensorId = msg.SensorId,
            Value = msg.Value,
            Unit = msg.Unit,
            Timestamp = msg.Timestamp
        };
        
        _db.SensorReadings.Add(reading);
        await _db.SaveChangesAsync();
    }
}