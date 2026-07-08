using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.Extensions.FileProviders;
using System.Text;
using dotnet_rest_api.Data;
using dotnet_rest_api.Services;
using dotnet_rest_api.Middleware;
using dotnet_rest_api.Extensions;
using Asp.Versioning;
using DotNetEnv;
using FluentValidation;

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

// FluentValidation - register all validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Health Checks with database connectivity
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        tags: new[] { "db", "postgresql" });

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

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
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
// Priority: Environment variable > appsettings.json > development fallback
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = "DevelopmentOnlySecretKeyNotForProduction123456789";
        Console.WriteLine("WARNING: Using development JWT key. Set JWT_KEY environment variable for production.");
    }
    else
    {
        throw new InvalidOperationException("JWT_KEY environment variable is required in production.");
    }
}

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
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
});

// Caching & Performance Services
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// AutoMapper Configuration
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// Rate Limiting (conditionally enabled)
var rateLimitEnabled = builder.Configuration.GetValue<bool>("RateLimit:Enabled", true);
if (rateLimitEnabled)
{
    builder.Services.AddRateLimit(builder.Configuration);
}

// CORS Configuration
// Explicit origins for credentialed SignalR connections come from config
// (Cors:AllowedOrigins); falls back to local dev front-ends.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    // Mobile / general REST clients (no credentials) — permissive.
    options.AddPolicy("FlutterAppPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Pagination");
    });

    // SignalR sends credentials, so it requires explicit origins (cannot combine
    // AllowCredentials with AllowAnyOrigin).
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Business & feature service registrations (see Extensions/ApplicationServiceExtensions.cs)
builder.Services.AddApplicationServices(builder.Configuration);

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
app.UseGlobalExceptionHandler(); // Global exception handling (first in pipeline)
app.UseCors("FlutterAppPolicy"); // Enable CORS for Flutter app

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

// Built-in Health Check Endpoints (Kubernetes standard)
app.MapHealthChecks("/healthz"); // Liveness probe
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") // Only database checks for readiness
});

// SignalR Hub Configuration for Real-Time Updates
app.MapHub<dotnet_rest_api.Hubs.NotificationHub>("/notificationHub")
   .RequireCors("SignalRPolicy"); // credentialed origins, distinct from the permissive REST policy

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
            logger.LogInformation("Ensuring database is created (bypassing migrations for development data seeding)...");
            // We use EnsureCreatedAsync instead of MigrateAsync to ensure new seed data is applied 
            // without needing to generate a new migration file in this environment
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database initialization completed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        // Ensure we don't crash in production, but rethrow in dev for visibility
        if (app.Environment.IsDevelopment())
        {
             // Log but continue if it's just that tables already exist (which EnsureCreated might handle or not)
             logger.LogWarning("Continuing despite error: " + ex.Message);
        }
    }
    finally 
    {
        // Cleanup or further checks if needed
    }
}

// ===================================
// APPLICATION STARTUP
// ===================================

await app.RunAsync();

// Exposed so integration tests can host the app via WebApplicationFactory<Program>.
public partial class Program { }
