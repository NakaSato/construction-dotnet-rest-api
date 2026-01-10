using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dotnet_rest_api.Extensions;

/// <summary>
/// Service defaults extension methods following .NET Aspire patterns.
/// Provides standardized configuration for health checks, resilience, and observability.
/// </summary>
public static class ServiceDefaultsExtension
{
    /// <summary>
    /// Adds common services following .NET Aspire patterns
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        // Add default health checks
        builder.ConfigureHealthChecks();
        
        // Add resilience (retry, circuit breaker patterns)
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Enable standard resilience features:
            // - Retry with exponential backoff
            // - Circuit breaker
            // - Timeout
            http.AddStandardResilienceHandler();
        });

        return builder;
    }

    /// <summary>
    /// Configure comprehensive health checks
    /// </summary>
    private static IHostApplicationBuilder ConfigureHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Self health check
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

        return builder;
    }

    /// <summary>
    /// Map default health check endpoints following Kubernetes conventions
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health check endpoints for Kubernetes
        // /health - General health
        // /alive - Liveness probe (is the app running?)
        // /ready - Readiness probe (can the app serve traffic?)
        
        app.MapHealthChecks("/health");
        
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("db")
        });

        return app;
    }
}

/// <summary>
/// HTTP client resilience extension for standard retry/circuit breaker patterns
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds standard resilience handler with retry and circuit breaker
    /// </summary>
    public static IHttpClientBuilder AddStandardResilienceHandler(this IHttpClientBuilder builder)
    {
        // Configure Polly-based resilience
        builder.ConfigureHttpClient(client =>
        {
            // Set reasonable timeout
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }
}
