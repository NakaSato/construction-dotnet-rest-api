# Dashboard Implementation Guide

This guide provides code examples for implementing the dashboard features using the Master Plan Management API.

## 🎯 Table of Contents
1. [Gantt Chart Implementation](#gantt-chart-implementation)
2. [Weekly View Implementation](#weekly-view-implementation)
3. [Progress Tracking](#progress-tracking)
4. [Real-time Updates](#real-time-updates)

## 📊 Gantt Chart Implementation

### TypeScript/React Example
```typescript
import { Gantt } from '@dhtmlx/gantt-react';

interface Task {
  id: string;
  text: string;
  start_date: string;
  end_date: string;
  progress: number;
  dependencies: string[];
}

const ProjectGantt: React.FC<{ planId: string }> = ({ planId }) => {
  const [tasks, setTasks] = useState<Task[]>([]);

  useEffect(() => {
    const fetchGanttData = async () => {
      const response = await fetch(`/api/v1/master-plans/${planId}/gantt-data`);
      const data = await response.json();
      setTasks(data.tasks);
    };
    fetchGanttData();
  }, [planId]);

  return (
    <Gantt
      tasks={tasks}
      zoom="Days"
      onTaskUpdate={handleTaskUpdate}
      onLinkCreate={handleLinkCreate}
    />
  );
};
```

### C# Controller Implementation
```csharp
[ApiController]
[Route("api/v1/master-plans")]
public class MasterPlansController : ControllerBase
{
    private readonly IMasterPlanService _service;
    
    [HttpGet("{planId}/gantt-data")]
    public async Task<ActionResult<GanttDataResponse>> GetGanttData(
        string planId,
        [FromQuery] int? depth,
        [FromQuery] DateTime? baseline)
    {
        var result = await _service.GetGanttDataAsync(
            planId, 
            depth ?? int.MaxValue,
            baseline);
            
        return result.Match<ActionResult<GanttDataResponse>>(
            success => Ok(success),
            error => error.ToActionResult()
        );
    }
}
```

## 📅 Weekly View Implementation

### TypeScript/React Example
```typescript
interface WeekSummary {
  weekNumber: number;
  weekLabel: string;
  summary: {
    totalTasks: number;
    completedTasks: number;
    efficiency: number;
  };
  tasks: Task[];
}

const WeeklyView: React.FC<{ planId: string }> = ({ planId }) => {
  const [weeklyData, setWeeklyData] = useState<WeekSummary[]>([]);

  useEffect(() => {
    const fetchWeeklyData = async () => {
      const startDate = format(startOfWeek(new Date()), 'yyyy-MM-dd');
      const endDate = format(addWeeks(new Date(), 2), 'yyyy-MM-dd');
      
      const response = await fetch(
        `/api/v1/master-plans/${planId}/weekly-view?` +
        `start_date=${startDate}&end_date=${endDate}`
      );
      const data = await response.json();
      setWeeklyData(data.weeks);
    };
    fetchWeeklyData();
  }, [planId]);

  return (
    <div className="weekly-view">
      {weeklyData.map(week => (
        <WeekCard
          key={week.weekNumber}
          week={week}
          onTaskUpdate={handleTaskUpdate}
        />
      ))}
    </div>
  );
};
```

### C# Service Implementation
```csharp
public class MasterPlanService : IMasterPlanService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Result<WeeklyViewResponse>> GetWeeklyViewAsync(
        string planId,
        DateTime startDate,
        DateTime endDate,
        string timezone = "UTC")
    {
        var weeks = await _context.Tasks
            .Where(t => t.MasterPlanId == planId)
            .Where(t => t.StartDate >= startDate && t.EndDate <= endDate)
            .GroupBy(t => GetWeekNumber(t.StartDate))
            .Select(g => new WeekSummary
            {
                WeekNumber = g.Key,
                WeekLabel = GetWeekLabel(g.Key, timezone),
                Summary = new WeekMetrics
                {
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(t => t.Status == TaskStatus.Completed),
                    Efficiency = CalculateEfficiency(g)
                },
                Tasks = g.Select(t => new TaskSummary(t)).ToList()
            })
            .ToListAsync();
            
        return Result.Success(new WeeklyViewResponse
        {
            PlanId = planId,
            ViewStartDate = startDate,
            ViewEndDate = endDate,
            Weeks = weeks
        });
    }
}
```

## 📈 Progress Tracking

### TypeScript/React Example
```typescript
interface ProgressData {
  percentageComplete: number;
  status: string;
  details: string;
  lastUpdated: string;
  metrics: {
    plannedHours: number;
    actualHours: number;
    efficiency: number;
  };
}

const ProgressCard: React.FC<{ taskId: string }> = ({ taskId }) => {
  const [progress, setProgress] = useState<ProgressData | null>(null);

  useEffect(() => {
    const subscription = webSocket
      .subscribe(`progress_updates/${taskId}`, (update) => {
        setProgress(update.progress);
      });
      
    return () => subscription.unsubscribe();
  }, [taskId]);

  return (
    <div className="progress-card">
      <CircularProgress value={progress?.percentageComplete ?? 0} />
      <StatusBadge status={progress?.status ?? 'Unknown'} />
      <MetricsPanel metrics={progress?.metrics} />
    </div>
  );
};
```

### C# WebSocket Handler Implementation
```csharp
public class ProgressWebSocketHandler : WebSocketHandler
{
    private readonly IMasterPlanService _service;
    private readonly ILogger<ProgressWebSocketHandler> _logger;
    
    public override async Task OnConnected(WebSocket socket)
    {
        var planId = Context.Request.Query["planId"].ToString();
        await _service.RegisterProgressListener(planId, async (update) =>
        {
            if (socket.State == WebSocketState.Open)
            {
                var message = JsonSerializer.Serialize(new
                {
                    type = "progress_update",
                    taskId = update.TaskId,
                    progress = update.Progress
                });
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes(message),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        });
    }
}
```

## 🔄 Real-time Updates

### TypeScript/React Example with Error Handling
```typescript
const useWebSocketConnection = (planId: string) => {
  const [connection, setConnection] = useState<WebSocket | null>(null);
  const reconnectAttempts = useRef(0);
  const maxReconnectAttempts = 5;

  const connect = useCallback(() => {
    if (reconnectAttempts.current >= maxReconnectAttempts) {
      console.error('Max reconnection attempts reached');
      return;
    }

    const ws = new WebSocket(
      `ws://api.example.com/ws/master-plans/${planId}/progress`
    );

    ws.onopen = () => {
      console.log('Connected to progress updates');
      reconnectAttempts.current = 0;
      setConnection(ws);
    };

    ws.onclose = (event) => {
      console.log('Connection closed:', event.code);
      setConnection(null);
      
      // Implement exponential backoff
      const timeout = Math.min(1000 * Math.pow(2, reconnectAttempts.current), 10000);
      reconnectAttempts.current++;
      
      setTimeout(() => connect(), timeout);
    };

    ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }, [planId]);

  useEffect(() => {
    connect();
    return () => connection?.close();
  }, [connect]);

  return connection;
};
```

### C# Progress Update Service Implementation
```csharp
public class ProgressUpdateService : IProgressUpdateService
{
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ICache _cache;
    
    public async Task UpdateProgressAsync(string taskId, ProgressUpdate update)
    {
        // Update progress in database
        await _context.Tasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Progress, update.Progress)
                .SetProperty(t => t.LastUpdated, DateTime.UtcNow)
            );
            
        // Cache the update
        await _cache.SetAsync(
            $"task_progress:{taskId}",
            update,
            TimeSpan.FromMinutes(5)
        );
            
        // Broadcast to all connected clients
        await _hubContext.Clients.Group(update.PlanId)
            .SendAsync("ProgressUpdate", new
            {
                taskId,
                progress = update
            });
    }
}
```

## 📱 Mobile Optimization Examples

### React Native Implementation
```typescript
const MobileWeeklyView: React.FC<{ planId: string }> = ({ planId }) => {
  const [data, setData] = useState<MobileWeekSummary[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      const response = await fetch(
        `/api/v1/master-plans/${planId}/weekly-view?view=mobile`
      );
      const result = await response.json();
      setData(result.weeks);
    };
    fetchData();
  }, [planId]);

  return (
    <FlatList
      data={data}
      renderItem={({ item }) => (
        <WeekSummaryCard
          progress={item.progress}
          status={item.status}
          criticalTasks={item.criticalTasks}
        />
      )}
      keyExtractor={(item) => item.weekLabel}
    />
  );
};
```

### Offline Support Implementation
```typescript
const useCachedData = <T>(key: string, fetcher: () => Promise<T>) => {
  const [data, setData] = useState<T | null>(null);

  useEffect(() => {
    const loadData = async () => {
      // Try to load from cache first
      const cached = await AsyncStorage.getItem(key);
      if (cached) {
        setData(JSON.parse(cached));
      }

      try {
        // Fetch fresh data
        const fresh = await fetcher();
        setData(fresh);
        await AsyncStorage.setItem(key, JSON.stringify(fresh));
      } catch (error) {
        console.error('Failed to fetch fresh data:', error);
        // We'll keep using cached data if available
      }
    };

    loadData();
  }, [key, fetcher]);

  return data;
};
```
