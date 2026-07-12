using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Users;

public interface IAuthService
{
    System.Threading.Tasks.Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    System.Threading.Tasks.Task<Result<UserDto>> RegisterAsync(RegisterRequest request);
    System.Threading.Tasks.Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken);
    System.Threading.Tasks.Task<Result<bool>> RevokeTokenAsync(string token);
    System.Threading.Tasks.Task<Result<bool>> ValidateTokenAsync(string token);
    System.Threading.Tasks.Task<Result<bool>> LogoutAsync(string token);
    System.Threading.Tasks.Task<Result<bool>> IsTokenBlacklistedAsync(string token);
    string? GetUserIdFromToken(string token);
    bool IsTokenExpired(string token);
}
