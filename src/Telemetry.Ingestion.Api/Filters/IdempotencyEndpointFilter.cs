using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;
using Telemetry.Contracts;

namespace Telemetry.Ingestion.Api.Filters;

public class IdempotencyEndpointFilter : IEndpointFilter
{
    private const string HeaderName = "Idempotency-Key";
    private const string KeyPrefix = "idempotency:";
    private static readonly TimeSpan TimeToLive = TimeSpan.FromHours(24);

    private readonly IConnectionMultiplexer _redis;
    
    public IdempotencyEndpointFilter(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        HttpContext http = context.HttpContext;

        Guid filterCorrelationId = Guid.NewGuid();
        http.Response.Headers["X-Correlation-Id"] = filterCorrelationId.ToString();

        if (!http.Request.Headers.TryGetValue(HeaderName, out StringValues raw) || raw.Count == 0)
        {
            return Results.Problem(
                title: "Idempotency-Key header is required.",
                statusCode:
                StatusCodes.Status400BadRequest);
        }

        if (!Guid.TryParse(raw.ToString(), out Guid key))
        {
            return Results.Problem(
                title: "Idempotency-Key must be a valid UUID.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        IDatabase db = _redis.GetDatabase();
        string redisKey = $"{KeyPrefix}{key}";
        
        RedisValue cached = await db.StringGetAsync(redisKey);
        if (cached.HasValue)
        {
            CachedResponse parsed = JsonSerializer.Deserialize<CachedResponse>((string)cached!)!;
            http.Response.Headers["X-Correlation-Id"] = parsed.CorrelationId;
            http.Response.Headers["Idempotent-Replayed"] = "true";
            return Results.Accepted(value: parsed.Body);
        }

        object? result = await next(context);

        if (result is Accepted<IngestionResponse> { Value: {} body})
        {
            CachedResponse toCache = new CachedResponse
            {
                CorrelationId = http.Response.Headers["X-Correlation-Id"].ToString(),
                Body = body,
            };
            
            await db.StringSetAsync(redisKey, JsonSerializer.Serialize(toCache), TimeToLive);
        }
        
        return result;
    }
}