using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using System.Text;
using dotnet_rest_api.Data;
using dotnet_rest_api.Services;
using dotnet_rest_api.Middleware;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// ===================================
// SERVICE REGISTRATION
// ===================================

// Basic Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=SolarProjectsDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    
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

// Caching & Performance Services
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddRateLimit(builder.Configuration);

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Business Services Registration
builder.Services.AddScoped<IDailyReportService, StubDailyReportService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Placeholder Services (temporary implementations)
builder.Services.AddScoped<IUserService, PlaceholderUserService>();
builder.Services.AddScoped<IProjectService, PlaceholderProjectService>();
builder.Services.AddScoped<IWorkRequestService, PlaceholderWorkRequestService>();
builder.Services.AddScoped<IResourceService, PlaceholderResourceService>();
builder.Services.AddScoped<IDocumentService, PlaceholderDocumentService>();
builder.Services.AddScoped<IImageService, PlaceholderImageService>();
builder.Services.AddScoped<ICalendarService, PlaceholderCalendarService>();
builder.Services.AddScoped<IWeeklyReportService, PlaceholderWeeklyReportService>();
builder.Services.AddScoped<IWeeklyWorkRequestService, PlaceholderWeeklyWorkRequestService>();
builder.Services.AddScoped<IWorkRequestApprovalService, PlaceholderWorkRequestApprovalService>();
builder.Services.AddScoped<INotificationService, PlaceholderNotificationService>();
builder.Services.AddScoped<IEmailService, PlaceholderEmailService>();
builder.Services.AddScoped<ICloudStorageService, PlaceholderCloudStorageService>();
builder.Services.AddScoped<IQueryService, PlaceholderQueryService>();

// ===================================
// APPLICATION PIPELINE CONFIGURATION
// ===================================

var app = builder.Build();

// Development Environment Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Solar Projects API V1");
        options.RoutePrefix = string.Empty;
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
app.UseRateLimit(); // Before authentication

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
app.UseAuthorization();
app.MapControllers();

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
        
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Ensuring database exists...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database initialization completed.");
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
