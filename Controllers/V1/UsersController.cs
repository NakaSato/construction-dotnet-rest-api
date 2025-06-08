using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing users
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
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
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            var result = await _userService.GetUsersAsync(pageNumber, pageSize, role);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users.");
        }
    }

    /// <summary>
    /// Gets a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user.");
        }
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User details</returns>
    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username cannot be empty.");

            var user = await _userService.GetUserByUsernameAsync(username);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"User with username '{username}' not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            return StatusCode(500, "An error occurred while retrieving the user.");
        }
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="createUserRequest">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.CreateUserAsync(createUserRequest);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.UserId }, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user.");
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
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] CreateUserRequest updateUserRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(id, updateUserRequest);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Updated user</returns>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
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
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return StatusCode(500, "An error occurred while activating the user.");
        }
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Updated user</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
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
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, "An error occurred while deactivating the user.");
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }

    /// <summary>
    /// Gets all users with advanced querying capabilities including filtering, sorting, and field selection
    /// </summary>
    /// <param name="parameters">Advanced query parameters</param>
    /// <returns>Enhanced paginated list of users with metadata</returns>
    [HttpGet("advanced")]
    public async Task<ActionResult<EnhancedPagedResult<UserDto>>> GetUsersAdvanced([FromQuery] UserQueryParameters parameters)
    {
        try
        {
            // Validate pagination parameters
            if (parameters.PageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (parameters.PageSize < 1 || parameters.PageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            // Parse filters from query string if not already populated
            if (!parameters.Filters.Any() && Request.Query.Any())
            {
                parameters.Filters = ParseFiltersFromQuery(Request.Query);
            }

            var result = await _userService.GetUsersAsync(parameters);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with advanced query");
            return StatusCode(500, "An error occurred while retrieving users.");
        }
    }

    /// <summary>
    /// Gets all users with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("rich")]
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
            // Validate pagination parameters
            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

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

    private List<FilterParameter> ParseFiltersFromQuery(IQueryCollection query)
    {
        var filters = new List<FilterParameter>();
        
        foreach (var kvp in query)
        {
            if (kvp.Key.StartsWith("filter."))
            {
                var parts = kvp.Key.Split('.');
                if (parts.Length >= 3)
                {
                    var field = parts[1];
                    var op = parts[2];
                    var value = kvp.Value.FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        filters.Add(new FilterParameter
                        {
                            Field = field,
                            Operator = op,
                            Value = value
                        });
                    }
                }
            }
        }
        
        return filters;
    }
}
