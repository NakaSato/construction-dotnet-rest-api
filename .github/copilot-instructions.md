# GitHub Copilot Instructions for .NET REST API

This project is a .NET 9.0 REST API for managing Todo items. Follow these instructions when working with this codebase.

## Project Overview

- **Framework**: .NET 9.0
- **Architecture**: Clean Architecture with Controllers, Services, and Data layers
- **Database**: Entity Framework Core with In-Memory Database
- **API Documentation**: Swagger/OpenAPI
- **Main Entity**: TodoItem

## Code Style Guidelines

### C# Conventions
- Use **file-scoped namespaces** (e.g., `namespace MyProject.Controllers;`)
- Use **nullable reference types** and handle null values appropriately
- Follow **PascalCase** for public members, **camelCase** for private/local variables
- Use **async/await** patterns for database operations
- Prefer **dependency injection** over static dependencies

### API Development
- Controllers should be lightweight and delegate business logic to services
- Use appropriate HTTP status codes (200, 201, 400, 404, etc.)
- Include proper error handling with meaningful error messages
- Use DTOs for data transfer when needed
- Follow RESTful conventions for endpoint naming

### Entity Framework
- Use `DbContext` for database operations
- Implement repository pattern when needed
- Use migrations for database schema changes
- Handle database exceptions appropriately

## Project Structure

```
/Controllers     - API controllers (TodoController.cs)
/Models         - Domain models (TodoItem.cs)
/Services       - Business logic (ITodoService.cs, TodoService.cs)
/Data           - Database context (TodoContext.cs)
/Program.cs     - Application entry point with DI configuration
```

## Common Patterns

### Controller Pattern
```csharp
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    
    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        var todos = await _todoService.GetAllTodosAsync();
        return Ok(todos);
    }
}
```

### Service Pattern
```csharp
public class TodoService : ITodoService
{
    private readonly TodoContext _context;
    
    public TodoService(TodoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<TodoItem>> GetAllTodosAsync()
    {
        return await _context.TodoItems.ToListAsync();
    }
}
```

## Development Guidelines

### When Adding New Features
1. **Models**: Create/update domain models in `/Models`
2. **Services**: Add business logic interfaces in `/Services`
3. **Data**: Update DbContext if new entities are added
4. **Controllers**: Create API endpoints following RESTful conventions
5. **DI**: Register new services in `Program.cs`

### When Adding New Endpoints
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
- Include proper route attributes
- Add input validation
- Handle errors gracefully
- Document with XML comments for Swagger

### Database Operations
- Use async methods (`async/await`)
- Handle `DbUpdateException` and other EF exceptions
- Use transactions for complex operations
- Implement proper error logging

### Testing Considerations
- Write unit tests for services
- Use in-memory database for testing
- Mock external dependencies
- Test both success and error scenarios

## Package Dependencies

Current NuGet packages:
- `Microsoft.EntityFrameworkCore` (9.0.0)
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (9.0.0)
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson` (9.0.0)
- `Swashbuckle.AspNetCore` (6.8.0)

## API Endpoints

Current Todo API endpoints:
- `GET /api/todo` - Get all todos
- `GET /api/todo/{id}` - Get todo by ID
- `POST /api/todo` - Create new todo
- `PUT /api/todo/{id}` - Update existing todo
- `DELETE /api/todo/{id}` - Delete todo

## Development Commands

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run --urls "http://localhost:5001"

# Run with watch mode for development
dotnet watch run --urls "http://localhost:5001"

# Add new migration (if using SQL Server)
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update
```

## Best Practices

1. **Always validate input** in controllers
2. **Use appropriate return types** (ActionResult<T>)
3. **Implement proper error handling** with try-catch blocks
4. **Use dependency injection** for loose coupling
5. **Follow async patterns** for I/O operations
6. **Write meaningful commit messages**
7. **Keep controllers thin** - move logic to services
8. **Use configuration** for environment-specific settings

## Security Considerations

- Validate all input parameters
- Use HTTPS in production
- Implement authentication/authorization when needed
- Sanitize data before database operations
- Log security events appropriately

Remember to keep this file updated as the project evolves and new patterns emerge.
