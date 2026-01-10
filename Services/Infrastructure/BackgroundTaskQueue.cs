namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Represents a background work item that can be queued for async processing
/// </summary>
public interface IBackgroundWorkItem
{
    /// <summary>
    /// Unique identifier for tracking
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Description of the work item
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// The work to be executed
    /// </summary>
    Func<IServiceProvider, CancellationToken, Task> WorkItem { get; }
    
    /// <summary>
    /// When the work item was queued
    /// </summary>
    DateTime QueuedAt { get; }
}

/// <summary>
/// Default implementation of a background work item
/// </summary>
public class BackgroundWorkItem : IBackgroundWorkItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Description { get; }
    public Func<IServiceProvider, CancellationToken, Task> WorkItem { get; }
    public DateTime QueuedAt { get; } = DateTime.UtcNow;

    public BackgroundWorkItem(string description, Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        Description = description;
        WorkItem = workItem;
    }
}

/// <summary>
/// Interface for queueing background tasks
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Queue a work item for background processing
    /// </summary>
    ValueTask QueueBackgroundWorkItemAsync(IBackgroundWorkItem workItem);
    
    /// <summary>
    /// Queue a simple async task
    /// </summary>
    ValueTask QueueBackgroundWorkItemAsync(string description, Func<IServiceProvider, CancellationToken, Task> workItem);
    
    /// <summary>
    /// Dequeue a work item (used by the hosted service)
    /// </summary>
    ValueTask<IBackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Get the current queue count
    /// </summary>
    int Count { get; }
}

/// <summary>
/// Default implementation of background task queue using Channel
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly System.Threading.Channels.Channel<IBackgroundWorkItem> _queue;
    private readonly ILogger<BackgroundTaskQueue> _logger;

    public BackgroundTaskQueue(int capacity, ILogger<BackgroundTaskQueue> logger)
    {
        _logger = logger;
        
        // Bounded channel with specified capacity
        var options = new System.Threading.Channels.BoundedChannelOptions(capacity)
        {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
        };
        _queue = System.Threading.Channels.Channel.CreateBounded<IBackgroundWorkItem>(options);
    }

    public int Count => _queue.Reader.Count;

    public async ValueTask QueueBackgroundWorkItemAsync(IBackgroundWorkItem workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        
        await _queue.Writer.WriteAsync(workItem);
        _logger.LogInformation("Queued background work item: {Description} (ID: {Id})", 
            workItem.Description, workItem.Id);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(string description, Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        var item = new BackgroundWorkItem(description, workItem);
        await QueueBackgroundWorkItemAsync(item);
    }

    public async ValueTask<IBackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
