using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
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
                return BadRequest(new ApiResponse<LoginResponse> { Success = false, Message = "Invalid input data" });
            }

            var result = await _authService.LoginAsync(request);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<LoginResponse>(_logger, ex, "user login");
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
                return BadRequest(new ApiResponse<UserDto> { Success = false, Message = "Invalid request data" });
            }

            var result = await _authService.RegisterAsync(request);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<UserDto>(_logger, ex, "user registration");
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

            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<string>(_logger, ex, "token refresh");
        }
    }

    /// <summary>
    /// User logout - invalidates the current JWT token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        try
        {
            // Extract token from Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Authorization header is missing or invalid"
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var result = await _authService.LogoutAsync(token);

            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "user logout");
        }
    }
}
