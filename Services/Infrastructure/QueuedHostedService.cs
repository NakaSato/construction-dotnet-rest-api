namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Hosted service that processes background work items from the queue
/// </summary>
public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting.");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IBackgroundWorkItem? workItem = null;
            
            try
            {
                workItem = await _taskQueue.DequeueAsync(stoppingToken);
                
                _logger.LogInformation(
                    "Processing background work item: {Description} (ID: {Id}, Queued: {QueuedAt})",
                    workItem.Description, workItem.Id, workItem.QueuedAt);

                // Create a scope for dependency injection
                using var scope = _scopeFactory.CreateScope();
                
                // Execute the work item
                await workItem.WorkItem(scope.ServiceProvider, stoppingToken);
                
                _logger.LogInformation(
                    "Completed background work item: {Description} (ID: {Id})",
                    workItem.Description, workItem.Id);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping token is triggered
                _logger.LogInformation("Background processing was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error occurred executing background work item: {Description} (ID: {Id})",
                    workItem?.Description ?? "Unknown", workItem?.Id);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping. Queue count: {Count}", _taskQueue.Count);
        await base.StopAsync(stoppingToken);
    }
}
