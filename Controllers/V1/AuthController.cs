using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse("Invalid input data", 400);
            }

            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 401);
            }

            return CreateSuccessResponse(result.Data!, "Login successful");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "user login");
        }
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse("Invalid request data", 400);
            }

            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data!, "User registered successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "user registration");
        }
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            
            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 401);
            }

            return CreateSuccessResponse(result.Data!, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "token refresh");
        }
    }
}
