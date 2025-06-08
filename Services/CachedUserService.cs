using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

/// <summary>
/// Enhanced User Service with comprehensive caching capabilities
/// </summary>
public class CachedUserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedUserService> _logger;

    // Cache duration constants
    private static readonly TimeSpan UserDetailsCacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan UserListCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RolesCacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan UserQueryCacheDuration = TimeSpan.FromMinutes(10);

    public CachedUserService(
        ApplicationDbContext context, 
        IQueryService queryService,
        ICacheService cacheService,
        ILogger<CachedUserService> logger)
    {
        _context = context;
        _queryService = queryService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var cacheKey = $"user:id:{userId}";
            
            // Try to get from cache first
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogDebug("Cache hit for user ID: {UserId}", userId);
                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = cachedUser
                };
            }

            _logger.LogDebug("Cache miss for user ID: {UserId}", userId);

            // Get from database
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userDto = MapToDto(user);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, userDto, UserDetailsCacheDuration);
            
            // Also cache by username for faster lookups
            var usernameCacheKey = $"user:username:{user.Username.ToLower()}";
            await _cacheService.SetAsync(usernameCacheKey, userDto, UserDetailsCacheDuration);

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = $"Error retrieving user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
    {
        try
        {
            var cacheKey = $"user:username:{username.ToLower()}";
            
            // Try to get from cache first
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogDebug("Cache hit for username: {Username}", username);
                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = cachedUser
                };
            }

            _logger.LogDebug("Cache miss for username: {Username}", username);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userDto = MapToDto(user);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, userDto, UserDetailsCacheDuration);
            
            // Also cache by ID for consistency
            var idCacheKey = $"user:id:{user.UserId}";
            await _cacheService.SetAsync(idCacheKey, userDto, UserDetailsCacheDuration);

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = $"Error retrieving user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? role = null)
    {
        try
        {
            var cacheKey = $"users:list:page:{pageNumber}:size:{pageSize}:role:{role ?? "all"}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<PagedResult<UserDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for users list: {CacheKey}", cacheKey);
                return new ApiResponse<PagedResult<UserDto>>
                {
                    Success = true,
                    Data = cachedResult
                };
            }

            _logger.LogDebug("Cache miss for users list: {CacheKey}", cacheKey);

            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role.RoleName.ToLower() == role.ToLower());
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    RoleName = u.Role.RoleName,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            var result = new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, UserListCacheDuration);

            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users list");
            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = false,
                Message = $"Error retrieving users: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters)
    {
        try
        {
            // Create cache key based on query parameters
            var cacheKey = $"users:query:{parameters.GetHashCode()}:{parameters.PageNumber}:{parameters.PageSize}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<EnhancedPagedResult<UserDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for users query: {CacheKey}", cacheKey);
                return new ApiResponse<EnhancedPagedResult<UserDto>>
                {
                    Success = true,
                    Data = cachedResult
                };
            }

            _logger.LogDebug("Cache miss for users query: {CacheKey}", cacheKey);

            var baseQuery = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Apply entity-specific filters first
            var filteredQuery = ApplyUserFilters(baseQuery, parameters);

            // Use the generic query service for advanced filtering, sorting, and pagination
            var result = await _queryService.ExecuteQueryAsync(filteredQuery, parameters);

            // Convert entities to DTOs
            var dtoItems = result.Items.Select(MapToDto).ToList();
            
            // Apply field selection if requested
            var finalItems = string.IsNullOrEmpty(parameters.Fields) 
                ? dtoItems.Cast<object>().ToList()
                : _queryService.ApplyFieldSelection(dtoItems, parameters.Fields);

            var enhancedResult = new EnhancedPagedResult<UserDto>
            {
                Items = string.IsNullOrEmpty(parameters.Fields) 
                    ? dtoItems 
                    : finalItems.Cast<UserDto>().ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder,
                RequestedFields = string.IsNullOrEmpty(parameters.Fields) 
                    ? new List<string>() 
                    : parameters.Fields.Split(',').Select(f => f.Trim()).ToList(),
                Metadata = result.Metadata
            };

            // Cache the result with shorter duration for complex queries
            await _cacheService.SetAsync(cacheKey, enhancedResult, UserQueryCacheDuration);

            return new ApiResponse<EnhancedPagedResult<UserDto>>
            {
                Success = true,
                Data = enhancedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with enhanced query");
            return new ApiResponse<EnhancedPagedResult<UserDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersLegacyAsync(int pageNumber = 1, int pageSize = 10, string? role = null)
    {
        // Use the cached version of the standard method
        return await GetUsersAsync(pageNumber, pageSize, role);
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username already exists (use cache if available)
            var existingUserByUsername = await GetUserByUsernameAsync(request.Username);
            if (existingUserByUsername.Success)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            // Check email existence in database (no cache needed for creation validation)
            var existingUserByEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUserByEmail != null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Verify role exists (use cache if available)
            var roleCacheKey = $"role:id:{request.RoleId}";
            var cachedRole = await _cacheService.GetAsync<string>(roleCacheKey);
            
            if (cachedRole == null)
            {
                var role = await _context.Roles.FindAsync(request.RoleId);
                if (role == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Invalid role specified"
                    };
                }
                // Cache role for future use
                await _cacheService.SetAsync(roleCacheKey, role.RoleName, RolesCacheDuration);
                cachedRole = role.RoleName;
            }

            var user = new Models.User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                RoleId = request.RoleId,
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
                RoleName = cachedRole,
                IsActive = user.IsActive
            };

            // Cache the new user
            var userIdCacheKey = $"user:id:{user.UserId}";
            var usernameCacheKey = $"user:username:{user.Username.ToLower()}";
            await _cacheService.SetAsync(userIdCacheKey, userDto, UserDetailsCacheDuration);
            await _cacheService.SetAsync(usernameCacheKey, userDto, UserDetailsCacheDuration);

            // Invalidate user lists cache
            await InvalidateUserListCache();

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto,
                Message = "User created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = $"Error creating user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, CreateUserRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if username/email already exists for other users
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId != userId && 
                    (u.Username == request.Username || u.Email == request.Email));

            if (existingUser != null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Username or email already exists"
                };
            }

            // Verify role exists
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid role specified"
                };
            }

            var oldUsername = user.Username; // Store for cache invalidation

            user.Username = request.Username;
            user.Email = request.Email;
            user.FullName = request.FullName;
            user.RoleId = request.RoleId;

            // Only update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = role.RoleName,
                IsActive = user.IsActive
            };

            // Update cache with new data
            var userIdCacheKey = $"user:id:{userId}";
            var newUsernameCacheKey = $"user:username:{user.Username.ToLower()}";
            var oldUsernameCacheKey = $"user:username:{oldUsername.ToLower()}";

            await _cacheService.SetAsync(userIdCacheKey, userDto, UserDetailsCacheDuration);
            await _cacheService.SetAsync(newUsernameCacheKey, userDto, UserDetailsCacheDuration);

            // Remove old username cache if username changed
            if (!oldUsername.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheService.RemoveAsync(oldUsernameCacheKey);
            }

            // Invalidate user lists cache
            await InvalidateUserListCache();

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto,
                Message = "User updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = $"Error updating user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if user has associated projects or tasks
            var hasProjects = await _context.Projects.AnyAsync(p => p.ProjectManagerId == userId);
            var hasTasks = await _context.ProjectTasks.AnyAsync(t => t.AssignedTechnicianId == userId);

            if (hasProjects || hasTasks)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete user with associated projects or tasks. Please reassign them first."
                };
            }

            var username = user.Username; // Store for cache invalidation

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Remove from cache
            var userIdCacheKey = $"user:id:{userId}";
            var usernameCacheKey = $"user:username:{username.ToLower()}";
            await _cacheService.RemoveAsync(userIdCacheKey);
            await _cacheService.RemoveAsync(usernameCacheKey);

            // Invalidate user lists cache
            await InvalidateUserListCache();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "User deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid userId, bool isActive)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            // Invalidate user caches
            var userIdCacheKey = $"user:id:{userId}";
            var usernameCacheKey = $"user:username:{user.Username.ToLower()}";
            await _cacheService.RemoveAsync(userIdCacheKey);
            await _cacheService.RemoveAsync(usernameCacheKey);

            // Invalidate user lists cache
            await InvalidateUserListCache();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"User {(isActive ? "activated" : "deactivated")} successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status: {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error updating user status: {ex.Message}"
            };
        }
    }

    #region Private Helper Methods

    private async Task InvalidateUserListCache()
    {
        // Invalidate all user list cache entries
        await _cacheService.InvalidateByPatternAsync("users:list:*");
        await _cacheService.InvalidateByPatternAsync("users:query:*");
    }

    private IQueryable<Models.User> ApplyUserFilters(IQueryable<Models.User> query, UserQueryParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.Username))
        {
            query = query.Where(u => u.Username.Contains(parameters.Username));
        }
        
        if (!string.IsNullOrEmpty(parameters.Email))
        {
            query = query.Where(u => u.Email.Contains(parameters.Email));
        }
        
        if (!string.IsNullOrEmpty(parameters.FullName))
        {
            query = query.Where(u => u.FullName.Contains(parameters.FullName));
        }
        
        if (!string.IsNullOrEmpty(parameters.Role))
        {
            query = query.Where(u => u.Role.RoleName.ToLower() == parameters.Role.ToLower());
        }
        
        if (parameters.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == parameters.IsActive.Value);
        }
        
        return query;
    }

    private UserDto MapToDto(Models.User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role.RoleName,
            IsActive = user.IsActive
        };
    }

    #endregion
}
