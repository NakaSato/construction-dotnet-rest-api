using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;
using BCrypt.Net;

namespace dotnet_rest_api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public UserService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
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

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                IsActive = user.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto
            };
        }
        catch (Exception ex)
        {
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

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                IsActive = user.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto
            };
        }
        catch (Exception ex)
        {
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

            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = false,
                Message = $"Error retrieving users: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

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

            var user = new User
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

            // Load the user with role data
            var createdUser = await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.UserId == user.UserId);

            var userDto = new UserDto
            {
                UserId = createdUser.UserId,
                Username = createdUser.Username,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                RoleName = createdUser.Role.RoleName,
                IsActive = createdUser.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto,
                Message = "User created successfully"
            };
        }
        catch (Exception ex)
        {
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

            // Load the user with role data
            var updatedUser = await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.UserId == user.UserId);

            var userDto = new UserDto
            {
                UserId = updatedUser.UserId,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                FullName = updatedUser.FullName,
                RoleName = updatedUser.Role.RoleName,
                IsActive = updatedUser.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto,
                Message = "User updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = $"Error updating user: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> PatchUserAsync(Guid userId, PatchUserRequest request)
    {
        try
        {
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

            // Validate unique constraints for fields being updated
            if (request.Username != null || request.Email != null)
            {
                var query = _context.Users.Where(u => u.UserId != userId);
                
                if (request.Username != null)
                    query = query.Where(u => u.Username == request.Username);
                
                if (request.Email != null)
                    query = query.Where(u => u.Email == request.Email);

                var conflictingUser = await query.FirstOrDefaultAsync();
                if (conflictingUser != null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Username or email already exists"
                    };
                }
            }

            // Update only provided fields
            if (request.Username != null)
                user.Username = request.Username;

            if (request.Email != null)
                user.Email = request.Email;

            if (request.Password != null)
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (request.FullName != null)
                user.FullName = request.FullName;

            if (request.RoleId.HasValue)
            {
                // Verify role exists
                var role = await _context.Roles.FindAsync(request.RoleId.Value);
                if (role == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Invalid role specified"
                    };
                }
                user.RoleId = request.RoleId.Value;
            }

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            // Reload user with role data to ensure fresh data
            var updatedUser = await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.UserId == user.UserId);

            var userDto = new UserDto
            {
                UserId = updatedUser.UserId,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                FullName = updatedUser.FullName,
                RoleName = updatedUser.Role.RoleName,
                IsActive = updatedUser.IsActive
            };

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = userDto,
                Message = "User updated successfully"
            };
        }
        catch (Exception ex)
        {
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

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "User deleted successfully"
            };
        }
        catch (Exception ex)
        {
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

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"User {(isActive ? "activated" : "deactivated")} successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error updating user status: {ex.Message}"
            };
        }
    }
    
    // Advanced querying methods
    public async Task<ApiResponse<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters)
    {
        try
        {
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

            return new ApiResponse<EnhancedPagedResult<UserDto>>
            {
                Success = true,
                Data = enhancedResult
            };
        }
        catch (Exception ex)
        {
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
        // This method maintains backward compatibility with the original API
        return await GetUsersAsync(pageNumber, pageSize, role);
    }
    
    private IQueryable<User> ApplyUserFilters(IQueryable<User> query, UserQueryParameters parameters)
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

    private UserDto MapToDto(User user)
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
}