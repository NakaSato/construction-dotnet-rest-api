using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.Extensions.Caching.Memory;
using Task = System.Threading.Tasks.Task;

namespace dotnet_rest_api.Services.Users;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the JWT signing key from configuration. Program.cs writes the
    /// authoritative key (env JWT_KEY → appsettings → dev fallback) back into
    /// configuration at startup, so signing here always matches the validation
    /// parameters. Throws if unset rather than falling back to a hardcoded secret.
    /// </summary>
    private byte[] GetSigningKeyBytes()
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("JWT signing key is not configured (set JWT_KEY or Jwt:Key).");
        return Encoding.UTF8.GetBytes(key);
    }

    // Brute-force throttle: after MaxFailedLoginAttempts failures for the same
    // identifier the account is locked for LockoutDuration. Backed by IMemoryCache,
    // so it is per-instance (a supplement to, not a replacement for, the
    // Redis-backed RateLimitMiddleware).
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private static string LoginAttemptsKey(string identifier) => $"login_fail_{identifier.Trim().ToLowerInvariant()}";

    // Refresh tokens are opaque random strings, single-use, and rotated on every
    // refresh. Only their SHA-256 hash is persisted (RefreshTokens table).
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var attemptsKey = LoginAttemptsKey(request.Username);
            if (_cache.TryGetValue(attemptsKey, out int failedAttempts) && failedAttempts >= MaxFailedLoginAttempts)
            {
                _logger.LogWarning("Login blocked for {Username}: too many failed attempts", request.Username);
                return ServiceResult<LoginResponse>.ErrorResult(
                    "Account temporarily locked due to repeated failed login attempts. Please try again later.");
            }

            // First try to authenticate against database users
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => (u.Username == request.Username || u.Email == request.Username) && u.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Successful login clears the failure counter.
                _cache.Remove(attemptsKey);

                var token = GenerateJwtToken(user);
                var refreshToken = await IssueRefreshTokenAsync(user.UserId);

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

            // Failed attempt: increment the counter (absolute expiry resets the
            // lockout window on each miss).
            _cache.Set(attemptsKey, failedAttempts + 1, LockoutDuration);
            return ServiceResult<LoginResponse>.ErrorResult("Invalid username or password");
        }
        catch (Exception ex)
        {
            // Log the detail server-side; return a generic message so exception
            // internals are not disclosed to the client.
            _logger.LogError(ex, "Error during login for {Username}", request.Username);
            return ServiceResult<LoginResponse>.ErrorResult("An error occurred during login. Please try again.");
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

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ServiceResult<LoginResponse>.ErrorResult("Refresh token is required");

            var hash = HashRefreshToken(refreshToken);

            var stored = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (stored == null)
                return ServiceResult<LoginResponse>.ErrorResult("Invalid refresh token");

            // Replay detection: a token that was already rotated or explicitly
            // revoked is being reused. Revoke the whole family for that user as a
            // defensive measure and reject.
            if (stored.RevokedAt != null)
            {
                await RevokeAllUserRefreshTokensAsync(stored.UserId);
                _logger.LogWarning("Refresh token reuse detected for user {UserId}; all tokens revoked", stored.UserId);
                return ServiceResult<LoginResponse>.ErrorResult("Invalid refresh token");
            }

            if (DateTime.UtcNow >= stored.ExpiresAt)
                return ServiceResult<LoginResponse>.ErrorResult("Refresh token has expired");

            var user = stored.User;
            if (user == null || !user.IsActive)
                return ServiceResult<LoginResponse>.ErrorResult("User is no longer active");

            // Rotate: issue a fresh token and stamp the consumed row so a replay is
            // detectable.
            var newRefreshToken = await IssueRefreshTokenAsync(user.UserId, stored);

            var response = new LoginResponse
            {
                Token = GenerateJwtToken(user),
                RefreshToken = newRefreshToken,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = user.Role?.RoleName ?? "User",
                    IsActive = user.IsActive
                }
            };

            return ServiceResult<LoginResponse>.SuccessResult(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ServiceResult<LoginResponse>.ErrorResult("An error occurred while refreshing the token.");
        }
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

    public async Task<ServiceResult<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKeyBytes();
            
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
                return ServiceResult<bool>.ErrorResult("Token is blacklisted");
            }

            await System.Threading.Tasks.Task.CompletedTask; // Ensure async compliance
            return ServiceResult<bool>.SuccessResult(true, "Token is valid");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Token validation failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RevokeTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? token;
            var cacheKey = $"blacklisted_token_{tokenId}";
            
            // Add token to blacklist cache
            var expiration = jwtToken.ValidTo.Subtract(DateTime.UtcNow);
            if (expiration > TimeSpan.Zero)
            {
                _cache.Set(cacheKey, true, expiration);
            }

            await System.Threading.Tasks.Task.CompletedTask; // Ensure async compliance
            return ServiceResult<bool>.SuccessResult(true, "Token revoked successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Token revocation failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> IsTokenBlacklistedAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? token;
            var cacheKey = $"blacklisted_token_{tokenId}";
            
            var isBlacklisted = _cache.TryGetValue(cacheKey, out _);

            await System.Threading.Tasks.Task.CompletedTask; // Ensure async compliance
            return ServiceResult<bool>.SuccessResult(isBlacklisted, isBlacklisted ? "Token is blacklisted" : "Token is not blacklisted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Error checking token blacklist status: {ex.Message}");
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "id")?.Value;
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true; // If we can't read the token, consider it expired
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(GetSigningKeyBytes());
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

    /// <summary>
    /// Generates a cryptographically-random opaque refresh token, persists only its
    /// SHA-256 hash, and returns the raw value to the caller (shown to the client
    /// exactly once). When <paramref name="rotatedFrom"/> is supplied the consumed
    /// token is revoked and linked to the new one, enabling replay detection.
    /// </summary>
    private async Task<string> IssueRefreshTokenAsync(Guid userId, RefreshToken? rotatedFrom = null)
    {
        var rawToken = GenerateSecureToken();
        var hash = HashRefreshToken(rawToken);
        var now = DateTime.UtcNow;

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt = now.Add(RefreshTokenLifetime)
        });

        if (rotatedFrom != null)
        {
            rotatedFrom.RevokedAt = now;
            rotatedFrom.ReplacedByTokenHash = hash;
        }

        await _context.SaveChangesAsync();
        return rawToken;
    }

    private async Task RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var active = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var rt in active)
            rt.RevokedAt = now;

        if (active.Count > 0)
            await _context.SaveChangesAsync();
    }

    private static string GenerateSecureToken()
    {
        // 32 bytes of CSPRNG entropy, URL-safe Base64.
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes); // 44 chars, fits TokenHash(64)
    }
}