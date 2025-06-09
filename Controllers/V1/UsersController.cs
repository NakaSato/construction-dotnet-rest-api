using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing users
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IQueryService _queryService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IQueryService queryService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users with optional pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="role">Filter by role name</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [MediumCache] // 15 minute cache for user lists
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null)
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return validationResult;

            var result = await _userService.GetUsersAsync(pageNumber, pageSize, role);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving users");
        }
    }

    /// <summary>
    /// Gets a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [LongCache] // 1 hour cache for individual user details
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving user {id}");
        }
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User details</returns>
    [HttpGet("username/{username}")]
    [LongCache] // 1 hour cache for user lookups by username
    public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return CreateErrorResponse("Username cannot be empty", 400);

            var user = await _userService.GetUserByUsernameAsync(username);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return CreateNotFoundResponse($"User with username '{username}' not found");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving user by username {username}");
        }
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="createUserRequest">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _userService.CreateUserAsync(createUserRequest);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.UserId }, result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "creating user");
        }
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateUserRequest">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] CreateUserRequest updateUserRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _userService.UpdateUserAsync(id, updateUserRequest);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"updating user {id}");
        }
    }

    /// <summary>
    /// Partially updates an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="patchUserRequest">User partial update data</param>
    /// <returns>Updated user</returns>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<UserDto>> PatchUser(Guid id, [FromBody] PatchUserRequest patchUserRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _userService.PatchUserAsync(id, patchUserRequest);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"patching user {id}");
        }
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Updated user</returns>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<bool>> ActivateUser(Guid id)
    {
        try
        {
            var result = await _userService.ActivateUserAsync(id, true);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"activating user {id}");
        }
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Updated user</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<bool>> DeactivateUser(Guid id)
    {
        try
        {
            var result = await _userService.ActivateUserAsync(id, false);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"deactivating user {id}");
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [NoCache] // No caching for write operations
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"deleting user {id}");
        }
    }

    /// <summary>
    /// Gets all users with advanced querying capabilities including filtering, sorting, and field selection
    /// </summary>
    /// <param name="parameters">Advanced query parameters</param>
    /// <returns>Enhanced paginated list of users with metadata</returns>
    [HttpGet("advanced")]
    [ShortCache] // 5 minute cache for advanced user queries
    public async Task<ActionResult<EnhancedPagedResult<UserDto>>> GetUsersAdvanced([FromQuery] UserQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetUsersAdvanced", parameters);

            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(parameters.PageNumber, parameters.PageSize);
            if (validationResult != null)
                return BadRequest(validationResult);

            // Apply filters from query string using base controller method
            var filtersString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filtersString);

            var result = await _userService.GetUsersAsync(parameters);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "GetUsersAdvanced");
        }
    }

    /// <summary>
    /// Gets all users with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("rich")]
    [ShortCache] // 5 minute cache for rich user pagination
    public async Task<ActionResult<ApiResponseWithPagination<UserDto>>> GetUsersWithRichPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(page, pageSize);
            if (validationResult != null)
                return validationResult;

            // Create query parameters
            var parameters = new UserQueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                Role = role,
                IsActive = isActive,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Get data using existing service
            var serviceResult = await _userService.GetUsersAsync(parameters);
            if (!serviceResult.Success)
                return BadRequest(serviceResult.Message);

            // Build base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(role)) queryParams.Add("role", role);
            if (isActive.HasValue) queryParams.Add("isActive", isActive.Value.ToString());
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add("sortBy", sortBy);
            if (!string.IsNullOrEmpty(sortOrder)) queryParams.Add("sortOrder", sortOrder);

            // Create rich paginated response using QueryService
            var response = _queryService.CreateRichPaginatedResponse(
                serviceResult.Data!.Items,
                serviceResult.Data.TotalCount,
                page,
                pageSize,
                baseUrl,
                queryParams,
                "Users retrieved successfully"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseWithPagination<UserDto>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
