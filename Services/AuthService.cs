using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace dotnet_rest_api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache)
    {
        _context = context;
        _configuration = configuration;
        _cache = cache;
        
        // Debug: Log the connection string being used
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"[DEBUG] AuthService connection string: {connectionString}");
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            // First try to authenticate against database users
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => (u.Username == request.Username || u.Email == request.Username) && u.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = user.Role?.RoleName ?? "User",
                    IsActive = user.IsActive
                };

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userDto
                };

                return ServiceResult<LoginResponse>.SuccessResult(response, "Login successful");
            }

            // Fallback for testing purposes - hardcoded admin credentials
            if (request.Username == "admin@example.com" && request.Password == "Admin123!")
            {
                var token = GenerateJwtTokenForTestUser();
                var response = new LoginResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        UserId = Guid.NewGuid(),
                        Username = "admin",
                        Email = "admin@example.com",
                        FullName = "Admin User",
                        RoleName = "Administrator",
                        IsActive = true
                    }
                };

                return ServiceResult<LoginResponse>.SuccessResult(response, "Login successful");
            }

            return ServiceResult<LoginResponse>.ErrorResult("Invalid username or password");
        }
        catch (Exception ex)
        {
            return ServiceResult<LoginResponse>.ErrorResult($"An error occurred during login: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser)
            {
                return ServiceResult<UserDto>.ErrorResult("Username or email already exists");
            }

            // Validate role exists or use default role
            int roleId = request.RoleId > 0 ? request.RoleId : 3; // Default to User role (ID 3)
            string roleName = "User"; // Default role name

            // Try to get the actual role name from database, but don't fail if it doesn't exist
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role != null)
                {
                    roleName = role.RoleName;
                }
            }
            catch
            {
                // If role lookup fails, just use default role name
                roleName = "User";
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = roleName,
                IsActive = user.IsActive
            };

            return ServiceResult<UserDto>.SuccessResult(userDto, "User registered successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken)
    {
        // For simplicity, we'll just return a new token
        // In production, you should store and validate refresh tokens
        await System.Threading.Tasks.Task.CompletedTask;
        
        return ServiceResult<string>.ErrorResult("Refresh token functionality not implemented");
    }

    public async Task<ServiceResult<bool>> LogoutAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return ServiceResult<bool>.ErrorResult("Token is required");
            }

            // Extract token ID for blacklisting
            var tokenHandler = new JwtSecurityTokenHandler();
            
            if (!tokenHandler.CanReadToken(token))
            {
                return ServiceResult<bool>.ErrorResult("Invalid token format");
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? token;
            
            // Add token to blacklist with expiration matching token expiration
            var expiration = jwtToken.ValidTo;
            var cacheKey = $"blacklisted_token_{tokenId}";
            
            _cache.Set(cacheKey, true, expiration);

            await System.Threading.Tasks.Task.CompletedTask; // Ensure async compliance
            
            return ServiceResult<bool>.SuccessResult(true, "Logout successful");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"An error occurred during logout: {ex.Message}");
        }
    }

    public bool ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction123456789");
            
            // First validate the token structure and signature
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "SolarProjectsAPI",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "SolarProjectsClient",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Check if token is blacklisted
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? token;
            var cacheKey = $"blacklisted_token_{tokenId}";
            
            if (_cache.TryGetValue(cacheKey, out _))
            {
                return false; // Token is blacklisted
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction123456789"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("id", user.UserId.ToString()),
            new Claim("jti", Guid.NewGuid().ToString()), // JWT ID for token tracking
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SolarProjectsAPI",
            audience: _configuration["Jwt:Audience"] ?? "SolarProjectsClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateJwtTokenForTestUser()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction123456789"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("id", Guid.NewGuid().ToString()),
            new Claim("jti", Guid.NewGuid().ToString()), // JWT ID for token tracking
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Administrator")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SolarProjectsAPI",
            audience: _configuration["Jwt:Audience"] ?? "SolarProjectsClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}