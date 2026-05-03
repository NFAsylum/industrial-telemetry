using Microsoft.EntityFrameworkCore;
using Telemetry.Domain;

namespace Telemetry.Infrastructure;

public class TelemetryDbContext : DbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : base(options) { }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReading>()
            .HasIndex(r => new { r.SensorId, r.Timestamp })
            .IsDescending(false, true);
    }
}
