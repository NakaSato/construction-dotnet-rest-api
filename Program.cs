using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using System.Text;
using dotnet_rest_api.Data;
using dotnet_rest_api.Services;
using dotnet_rest_api.Middleware;
using dotnet_rest_api.Extensions;
using Asp.Versioning;
using DotNetEnv;

// Load environment variables from .env file (local development)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel URLs based on environment
var aspnetcoreHttpPorts = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
var isDocker = builder.Environment.EnvironmentName.Equals("Docker", StringComparison.OrdinalIgnoreCase);

if (!string.IsNullOrEmpty(aspnetcoreHttpPorts) || isDocker)
{
    // In Docker or when ASPNETCORE_HTTP_PORTS is set, listen on all interfaces
    var port = aspnetcoreHttpPorts ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($"Configuring for Docker/Production: listening on http://0.0.0.0:{port}");
}
else if (builder.Environment.IsDevelopment())
{
    // Only use localhost:5001 for local development outside Docker
    builder.WebHost.UseUrls("http://localhost:5001");
    Console.WriteLine("Configuring for local development: listening on http://localhost:5001");
}

// ===================================
// SERVICE REGISTRATION
// ===================================

// Basic Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// SignalR Configuration with enhanced features
builder.Services.AddSignalR(options =>
{
    // Enable detailed errors in development
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    
    // Configure client timeout and keep alive
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
    options.KeepAliveInterval = TimeSpan.FromMinutes(2);
    
    // Set maximum message size (1MB)
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

// Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Solar Project Management API", 
        Version = "v1",
        Description = "RESTful API for managing solar projects, tasks, and daily reports"
    });

    // JWT authentication for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration

// Prefer environment variable for connection string, fallback to config, then default
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=SolarProjectsDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use in-memory database if environment variable is set or PostgreSQL is not available
    var useInMemory = Environment.GetEnvironmentVariable("USE_IN_MEMORY_DB")?.ToLower() == "true" 
        || builder.Environment.IsEnvironment("Test");
    
    if (useInMemory)
    {
        options.UseInMemoryDatabase("SolarProjectsInMemoryDb");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Authentication & Authorization Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SolarProjectsAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SolarProjectsClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure SignalR token authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // If the request is for SignalR hub
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// API Versioning Configuration
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// SignalR Configuration for Real-Time Updates
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Caching & Performance Services
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(Program));

// Rate Limiting (conditionally enabled)
var rateLimitEnabled = builder.Configuration.GetValue<bool>("RateLimit:Enabled", true);
if (rateLimitEnabled)
{
    builder.Services.AddRateLimit(builder.Configuration);
}

// CORS Configuration with SignalR support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Specific policy for SignalR with credentials
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Business Services Registration
builder.Services.AddScoped<IDailyReportService, StubDailyReportService>();

// WBS Services Registration
builder.Services.AddScoped<WbsDataSeeder>();
builder.Services.AddScoped<IWbsService, WbsService>();

// Core Services - New abstractions for better code quality
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IResponseBuilderService, ResponseBuilderService>();
builder.Services.AddScoped<IValidationHelperService, ValidationHelperService>();

// Service implementations
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>(); // Real implementation
builder.Services.AddScoped<ITaskService, TaskService>(); // Real implementation
builder.Services.AddScoped<IMasterPlanService, MasterPlanService>(); // Real implementation

// Add refactored master plan services with CQRS handlers (currently incomplete)
// builder.Services.AddAllRefactoredServices();

// Real Service Implementations
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IQueryService, QueryService>();

// Note: Placeholder services removed - implement these when needed:
// - IWorkRequestService, IResourceService, IDocumentService, IImageService
// - ICalendarService, IWeeklyReportService, IWeeklyWorkRequestService
// - IWorkRequestApprovalService, IEmailService, ICloudStorageService

// Background Services
builder.Services.AddNotificationBackgroundService();

// ===================================
// APPLICATION PIPELINE CONFIGURATION
// ===================================

var app = builder.Build();

// Swagger Configuration (enabled in Development and Docker for API documentation)
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Solar Projects API V1");
        options.RoutePrefix = string.Empty; // Makes Swagger UI available at root URL
    });
}

// HTTPS Redirection (conditional)
if (!app.Environment.IsDevelopment() || 
    builder.Configuration.GetValue<bool>("ForceHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

// Middleware Pipeline
app.UseCors();

// Rate Limiting Middleware (conditionally enabled)
if (rateLimitEnabled)
{
    app.UseRateLimit(); // Before authentication
}

// Static Files Configuration
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/files"
});

// Authentication & Authorization
app.UseAuthentication();
app.UseMiddleware<dotnet_rest_api.Middleware.JwtBlacklistMiddleware>();
app.UseAuthorization();
app.MapControllers();

// SignalR Hub Configuration for Real-Time Updates
app.MapHub<dotnet_rest_api.Hubs.NotificationHub>("/notificationHub");

// ===================================
// DATABASE INITIALIZATION
// ===================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var useInMemory = Environment.GetEnvironmentVariable("USE_IN_MEMORY_DB")?.ToLower() == "true" 
                         || app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);
        
        if (useInMemory)
        {
            logger.LogInformation("Using in-memory database - ensuring database is created...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("In-memory database initialized.");
        }
        else
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// ===================================
// APPLICATION STARTUP
// ===================================

await app.RunAsync();
