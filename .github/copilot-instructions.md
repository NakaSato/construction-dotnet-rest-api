# GitHub Copilot Instructions for Solar Projects API

This is a comprehensive .NET 9.0 REST API for solar project management with real-time capabilities, featuring JWT authentication, SignalR integration, and extensive project management functionality.

## 🏗️ Architecture Overview

**Core Stack**: .NET 9.0 + PostgreSQL + Entity Framework Core + SignalR + JWT Auth
**Domain**: Solar PV installation project management (WBS, Daily Reports, Work Requests)
**Pattern**: Clean Architecture with feature-based service organization

### Key Architectural Decisions
- **Feature Controllers**: Most controllers are `.disabled` - rename to `.cs` to activate
- **Real-time Integration**: SignalR hub at `/notificationHub` for live updates
- **JWT Blacklisting**: Custom middleware for token revocation in `JwtBlacklistMiddleware`
- **Service Result Pattern**: All services return `ServiceResult<T>` for consistent error handling
- **Base Controllers**: Inherit from `BaseApiController` for common functionality

## 🔐 Authentication & Security

### JWT Implementation
```csharp
// Authentication configured in Program.cs with SignalR support
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken) && 
            path.StartsWithSegments("/notificationHub"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

### Default Credentials (Development)
- **Admin**: `admin@example.com` / `Admin123!`
- **JWT Blacklisting**: Tokens stored in `IMemoryCache` with expiration

## 🚀 Real-Time Architecture

### SignalR Hub Pattern
```csharp
[Authorize]
public class NotificationHub : Hub
{
    // Group-based messaging for projects, regions, facilities
    await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
    await Clients.Group($"project_{projectId}").SendAsync("ProjectUpdate", data);
}
```

### Integration in Services
```csharp
// Pass user context from controllers to services for real-time notifications
var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
var result = await _service.CreateAsync(request, userId, userName);
```

## 📁 Critical File Patterns

### Service Registration (`Program.cs`)
- **Environment Detection**: Auto-configures URLs for Docker vs local development
- **Database Strategy**: PostgreSQL (production) or In-Memory (testing) based on env vars
- **Conditional Features**: Rate limiting, CORS policies, middleware pipeline

### Controller Activation
```bash
# Enable disabled controllers by renaming
mv Controllers/V1/ProjectsController.cs.disabled Controllers/V1/ProjectsController.cs
```

### Service Pattern
```csharp
public class AuthService : IAuthService
{
    // JWT generation with blacklist support
    private readonly IMemoryCache _cache;
    
    public async Task<ServiceResult<bool>> LogoutAsync(string token)
    {
        // Add to blacklist cache with token expiration
        _cache.Set($"blacklisted_token_{tokenId}", true, expiration);
    }
}
```

## 🔧 Development Workflows

### Docker Development
```bash
# Complete stack with PostgreSQL
docker-compose up -d

# API available at http://localhost:5001
# Database on localhost:5432
```

### Local Development
```bash
# Auto-configures to localhost:5001
dotnet watch run --urls "http://localhost:5001"

# Swagger UI at root: http://localhost:5001
```

### Feature Activation
1. **Enable Controllers**: Remove `.disabled` extension
2. **Register Services**: Add to `Program.cs` service registration
3. **Database Migrations**: Run `dotnet ef database update`

## 🎯 Project-Specific Patterns

### WBS (Work Breakdown Structure)
- **Hierarchical Tasks**: Parent-child relationships with unlimited nesting
- **Dependencies**: Task prerequisite validation and critical path analysis
- **Evidence Management**: Photo/document attachments per task
- **Installation Areas**: Carport, Water Tank Roof, Inverter Room, etc.

### Data Flow Architecture
```
Controller → Service (with user context) → Repository → Database
    ↓
SignalR Hub → Real-time client updates
```

### Error Handling Strategy
```csharp
// Consistent error responses using ServiceResult<T>
return ServiceResult<T>.ErrorResult("Message");
return ServiceResult<T>.SuccessResult(data, "Message");

// Controllers use ToApiResponse() extension
return ToApiResponse(result);
```

## 🧪 Testing & Development

### HTTP Test Files
- **Location**: `tests/http/` directory
- **Usage**: VS Code REST Client extension
- **Coverage**: All major endpoints with authentication

### Environment Configuration
- **Development**: Swagger enabled, detailed errors, in-memory database option
- **Docker**: Production-like setup with PostgreSQL
- **Variables**: `.env` file support for local development

## 🔑 Key Integration Points

### Mobile/Flutter Support
- **CORS**: Pre-configured for mobile apps
- **Mobile DTOs**: Lightweight data transfer objects in `/DTOs/MobileDTOs.cs`
- **Endpoints**: Optimized mobile endpoints for project data

### Real-time Features
- **Project Updates**: Automatic notifications for CRUD operations
- **Geographic Groups**: Regional grouping (Northern, Western, Central Thailand)
- **Role-based Groups**: Admin, ProjectManager, User group messaging

## ⚠️ Important Conventions

- **Never edit** disabled controllers directly - rename first
- **Always pass user context** (userId, userName) from controllers to services
- **Use AutoMapper** for entity-DTO transformations (configured in `MappingProfile.cs`)
- **Rate limiting** enabled by default - configure in `RateLimit:Enabled`
- **Cache attributes** available: `[ShortCache]`, `[MediumCache]`, `[LongCache]`

## 🔍 Debugging Tips

- **Health Check**: `GET /health` for API status
- **Swagger**: Available at root URL in development
- **SignalR**: Connection endpoint `/notificationHub` with JWT query param
- **Logs**: Detailed logging configured for development environment
