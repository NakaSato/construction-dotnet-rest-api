using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        
        // Debug: Log the connection string being used
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"[DEBUG] AuthService connection string: {connectionString}");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                IsActive = user.IsActive
            };

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = userDto
            };

            return new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Username or email already exists"
                };
            }

            // Validate role exists or use default role
            int roleId = request.RoleId;
            if (roleId == 0)
            {
                // Default to FieldTechnician role for new registrations
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "FieldTechnician");
                if (defaultRole == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Default role not found. Please contact administrator."
                    };
                }
                roleId = defaultRole.RoleId;
            }
            else
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
                if (!roleExists)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Invalid role specified"
                    };
                }
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

            var role = await _context.Roles.FindAsync(request.RoleId);
            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = role?.RoleName ?? "",
                IsActive = user.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User registered successfully",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An error occurred during registration",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> RefreshTokenAsync(string refreshToken)
    {
        // For simplicity, we'll just return a new token
        // In production, you should store and validate refresh tokens
        await Task.CompletedTask;
        
        return new ApiResponse<string>
        {
            Success = false,
            Message = "Refresh token functionality not implemented"
        };
    }

    public bool ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKey");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKey");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}
