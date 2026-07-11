using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Periodically purges expired refresh tokens so the RefreshTokens table stays
/// bounded. Only *expired* rows are removed — a revoked-but-unexpired token is
/// retained so <see cref="Services.Users.AuthService"/> can still detect a replay
/// of it (reuse of a rotated token revokes the whole family). Once expired, a
/// token is unusable regardless, so dropping it costs nothing.
/// </summary>
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(6);

    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Refresh token cleanup service started (interval {Interval})", SweepInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging expired refresh tokens");
            }

            try
            {
                await System.Threading.Tasks.Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Refresh token cleanup service stopped");
    }

    private async System.Threading.Tasks.Task PurgeExpiredAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow;
        var expired = await db.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoff)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return;

        db.RefreshTokens.RemoveRange(expired);
        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Purged {Count} expired refresh token(s)", expired.Count);
    }
}
