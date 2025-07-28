using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Users;

public interface IAuthService
{
    System.Threading.Tasks.Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request);
    System.Threading.Tasks.Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken);
    System.Threading.Tasks.Task<ServiceResult<bool>> RevokeTokenAsync(string token);
    System.Threading.Tasks.Task<ServiceResult<bool>> ValidateTokenAsync(string token);
    System.Threading.Tasks.Task<ServiceResult<bool>> LogoutAsync(string token);
    System.Threading.Tasks.Task<ServiceResult<bool>> IsTokenBlacklistedAsync(string token);
    string? GetUserIdFromToken(string token);
    bool IsTokenExpired(string token);
}
