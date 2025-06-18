# Master Plan API Implementation Guide

This guide provides detailed implementation instructions for the Master Plan Management API architecture.

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 15+
- Redis 6+
- Azure subscription (for cloud deployment)

### Project Setup
1. Clone the repository
2. Install dependencies
3. Configure environment variables
4. Run database migrations
5. Start the application

## Core Implementation Steps

### 1. Database Setup

#### Entity Framework Configuration
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<MasterPlan> MasterPlans { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MasterPlanConfiguration());
        // Apply other configurations
    }
}
```

#### Migration Commands
```bash
dotnet ef migrations add InitialMasterPlanSchema
dotnet ef database update
```

### 2. API Layer Implementation

#### Controller Structure
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class MasterPlansController : ControllerBase
{
    private readonly IMasterPlanService _service;
    private readonly ILogger<MasterPlansController> _logger;

    public MasterPlansController(
        IMasterPlanService service,
        ILogger<MasterPlansController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // Implement endpoints
}
```

### 3. Service Layer Implementation

#### Service Interface
```csharp
public interface IMasterPlanService
{
    Task<Result<MasterPlanDto>> CreateAsync(CreateMasterPlanDto dto);
    Task<Result<MasterPlanDto>> UpdateAsync(string id, UpdateMasterPlanDto dto);
    Task<Result<bool>> DeleteAsync(string id);
    Task<Result<MasterPlanDto>> GetByIdAsync(string id);
    Task<Result<IEnumerable<MasterPlanDto>>> GetAllAsync(QueryParameters parameters);
}
```

### 4. Caching Implementation

#### Redis Cache Configuration
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
    options.InstanceName = "MasterPlan_";
});
```

### 5. Message Queue Integration

#### Service Bus Configuration
```csharp
services.AddAzureServiceBus(options =>
{
    options.ConnectionString = Configuration.GetConnectionString("ServiceBus");
    options.QueueName = "masterplan-queue";
});
```

## Security Implementation

### 1. JWT Authentication

#### Configuration
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure JWT options
    });
```

### 2. Authorization

#### Policy Configuration
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("MasterPlanAdmin", policy =>
        policy.RequireRole("Admin"));
});
```

## Performance Optimization

### 1. Entity Framework Optimization

#### Query Optimization
```csharp
public async Task<IEnumerable<MasterPlan>> GetAllAsync()
{
    return await _context.MasterPlans
        .AsNoTracking()
        .Include(x => x.Tasks)
        .AsSplitQuery()
        .ToListAsync();
}
```

### 2. Caching Implementation

#### Cache Service
```csharp
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        // Implementation
    }
}
```

## Testing Guidelines

### 1. Unit Tests

#### Service Tests
```csharp
public class MasterPlanServiceTests
{
    private readonly IMasterPlanService _service;
    private readonly Mock<IRepository<MasterPlan>> _repository;
    
    [Fact]
    public async Task CreateMasterPlan_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateMasterPlanDto { /* ... */ };
        
        // Act
        var result = await _service.CreateAsync(dto);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

### 2. Integration Tests

#### API Tests
```csharp
public class MasterPlanApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    [Fact]
    public async Task CreateMasterPlan_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/master-plans");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## Deployment Guide

### 1. Azure Infrastructure Setup

#### Resource Creation
```bash
# Create Resource Group
az group create --name masterplan-rg --location eastus

# Create AKS Cluster
az aks create -g masterplan-rg -n masterplan-aks --node-count 3

# Create PostgreSQL Database
az postgres flexible-server create \
    --resource-group masterplan-rg \
    --name masterplan-db
```

### 2. CI/CD Pipeline

#### Azure DevOps Pipeline
```yaml
trigger:
  - main

stages:
  - stage: Build
    jobs:
      - job: Build
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: build
              projects: '**/*.csproj'
              
  - stage: Test
    jobs:
      - job: Test
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: test
              projects: '**/*Tests/*.csproj'
              
  - stage: Deploy
    jobs:
      - job: Deploy
        steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: '$(AzureSubscription)'
              appName: '$(WebAppName)'
```

## Monitoring and Logging

### 1. Application Insights Setup

#### Configuration
```csharp
services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = Configuration["ApplicationInsights:ConnectionString"];
});
```

### 2. Logging Configuration

#### Logging Setup
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddApplicationInsights();
        });
```

## Maintenance Guidelines

### 1. Database Maintenance

#### Index Maintenance
```sql
-- Rebuild indexes
REINDEX TABLE master_plans;

-- Update statistics
ANALYZE master_plans;
```

### 2. Monitoring Health

#### Health Checks
```csharp
services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(Configuration.GetConnectionString("Redis"))
    .AddAzureServiceBus(Configuration.GetConnectionString("ServiceBus"));
```

## Troubleshooting

### Common Issues and Solutions

1. **Performance Issues**
   - Check database indexes
   - Verify cache hit rates
   - Review query execution plans

2. **Connection Issues**
   - Verify connection strings
   - Check network security rules
   - Review SSL certificates

3. **Authentication Issues**
   - Verify JWT token configuration
   - Check role assignments
   - Review authorization policies
