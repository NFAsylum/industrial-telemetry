namespace Telemetry.Ingestion.Api.Endpoints;

public static class EndpointsRegister
{
    public static void RegisterEndpoints(WebApplication app)
    {
        ReadingsEndpoint.Register(app);
    }
}