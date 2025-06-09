using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using dotnet_rest_api.Data;
using dotnet_rest_api.Services;
using dotnet_rest_api.Middleware;
using Microsoft.Extensions.FileProviders;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PROJECT_API", 
        Version = "v1",
        Description = "RESTful API for managing solar projects, tasks, and images"
    });

    // Add JWT authentication to Swagger
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

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DefaultConnection string is not configured. Please set up PostgreSQL connection string.");
}

// Use PostgreSQL for main application data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
    
    // Enable logging for debugging connection issues
    options.LogTo(Console.WriteLine, LogLevel.Information);
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction";
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
});

builder.Services.AddAuthorization();

// Configure API Versioning
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

// Add caching services
builder.Services.AddMemoryCache();

// Configure Redis distributed cache (optional - will fallback to memory cache if not available)
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "SolarProjectsAPI";
    });
}
else
{
    // Fallback to distributed memory cache when Redis is not available
    builder.Services.AddDistributedMemoryCache();
}

// Register caching services
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Caching"));
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQueryService, QueryService>();

// Use cached versions of services for better performance
builder.Services.AddScoped<IProjectService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    var queryService = provider.GetRequiredService<IQueryService>();
    // Temporarily use ProjectService instead of CachedProjectService while migrating to Result<T>
    return new ProjectService(context, queryService);
    
    // TODO: Re-enable CachedProjectService after updating it to use Result<T>
    // var cacheService = provider.GetRequiredService<ICacheService>();
    // var logger = provider.GetRequiredService<ILogger<CachedProjectService>>();
    // return new CachedProjectService(context, queryService, cacheService, logger);
});

builder.Services.AddScoped<IUserService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    var queryService = provider.GetRequiredService<IQueryService>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    var logger = provider.GetRequiredService<ILogger<CachedUserService>>();
    return new CachedUserService(context, queryService, cacheService, logger);
});

builder.Services.AddScoped<ITaskService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    var queryService = provider.GetRequiredService<IQueryService>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    var logger = provider.GetRequiredService<ILogger<CachedTaskService>>();
    return new CachedTaskService(context, queryService, cacheService, logger);
});
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();

// Register Daily Report and Work Request services
builder.Services.AddScoped<IDailyReportService, DailyReportService>();
builder.Services.AddScoped<IWorkRequestService, WorkRequestService>();

// Register Data Seeder service
builder.Services.AddScoped<DataSeeder>();

// Add rate limiting services
builder.Services.AddRateLimit(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Solar Projects API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// Configure HTTPS redirection based on environment and configuration
if (!app.Environment.IsDevelopment() || 
    builder.Configuration.GetValue<bool>("ForceHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseCors();

// Add HTTP caching middleware for ETag and Cache-Control headers
app.UseMiddleware<HttpCacheMiddleware>();

// Add rate limiting middleware (before authentication)
app.UseRateLimit();

// Serve static files for uploaded images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/files"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply migrations for PostgreSQL (ApplicationDbContext)
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true")
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            
            // Seed sample data in development
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("Seeding sample data...");
                var dataSeeder = services.GetRequiredService<DataSeeder>();
                await dataSeeder.SeedSampleDataAsync();
                logger.LogInformation("Sample data seeded successfully.");
            }
        }
        else
        {
            // In production, ensure database exists but don't auto-migrate
            await context.Database.EnsureCreatedAsync();
        }
        
        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        // In development, we might want to continue even if DB is not available
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

await app.RunAsync();
