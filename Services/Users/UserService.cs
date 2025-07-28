using Microsoft.EntityFrameworkCore;
using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Users;

/// <summary>
/// User service implementation for managing users in the solar projects system
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string? role)
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
            var skip = (pageNumber - 1) * pageSize;

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            var result = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<UserDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with pageNumber: {PageNumber}, pageSize: {PageSize}, role: {Role}", 
                pageNumber, pageSize, role);
            return ServiceResult<PagedResult<UserDto>>.ErrorResult($"An error occurred during user retrieval: {ex.Message}");
        }
    }

    public async Task<ServiceResult<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Role))
            {
                query = query.Where(u => u.Role.RoleName.ToLower() == parameters.Role.ToLower());
            }

            if (!string.IsNullOrEmpty(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLower();
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(searchTerm) ||
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm));
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == parameters.IsActive.Value);
            }

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "fullname" => parameters.SortOrder == "desc" 
                    ? query.OrderByDescending(u => u.FullName)
                    : query.OrderBy(u => u.FullName),
                "username" => parameters.SortOrder == "desc"
                    ? query.OrderByDescending(u => u.Username)
                    : query.OrderBy(u => u.Username),
                "email" => parameters.SortOrder == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "createdat" => parameters.SortOrder == "desc"
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt),
                "role" => parameters.SortOrder == "desc"
                    ? query.OrderByDescending(u => u.Role.RoleName)
                    : query.OrderBy(u => u.Role.RoleName),
                _ => query.OrderBy(u => u.FullName)
            };

            var totalCount = await query.CountAsync();
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;

            var users = await query
                .Skip(skip)
                .Take(parameters.PageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            var result = new EnhancedPagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };

            return ServiceResult<EnhancedPagedResult<UserDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with parameters: {@Parameters}", parameters);
            return ServiceResult<EnhancedPagedResult<UserDto>>.ErrorResult($"An error occurred during user retrieval: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return ServiceResult<UserDto>.ErrorResult("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during user retrieval: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return ServiceResult<UserDto>.ErrorResult("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username: {Username}", username);
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during user retrieval: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUsername = await _context.Users
                .AnyAsync(u => u.Username == request.Username);
            
            if (existingUsername)
            {
                return ServiceResult<UserDto>.ErrorResult("Username already exists");
            }

            // Check if email already exists
            var existingEmail = await _context.Users
                .AnyAsync(u => u.Email == request.Email);
            
            if (existingEmail)
            {
                return ServiceResult<UserDto>.ErrorResult("Email already exists");
            }

            // Verify role exists
            var roleExists = await _context.Roles
                .AnyAsync(r => r.RoleId == request.RoleId);
            
            if (!roleExists)
            {
                return ServiceResult<UserDto>.ErrorResult("Invalid role specified");
            }

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load the user with role for DTO mapping
            var createdUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            var userDto = _mapper.Map<UserDto>(createdUser);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {@Request}", request);
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during user creation: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return ServiceResult<UserDto>.ErrorResult("User not found");
            }

            // Check for email conflicts (excluding current user)
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.UserId != id);
                
                if (emailExists)
                {
                    return ServiceResult<UserDto>.ErrorResult("Email already exists");
                }
                user.Email = request.Email;
            }

            // Update FullName from FirstName and LastName
            var fullName = $"{request.FirstName} {request.LastName}".Trim();
            if (!string.IsNullOrEmpty(fullName))
            {
                user.FullName = fullName;
            }

            // Update role if provided
            if (!string.IsNullOrEmpty(request.Role))
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == request.Role.ToLower());
                
                if (role == null)
                {
                    return ServiceResult<UserDto>.ErrorResult("Invalid role specified");
                }
                user.RoleId = role.RoleId;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            await _context.SaveChangesAsync();

            // Reload user with role for DTO mapping
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}, Request: {@Request}", id, request);
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during user update: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> PatchUserAsync(Guid id, PatchUserRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return ServiceResult<UserDto>.ErrorResult("User not found");
            }

            // Apply only the provided fields
            bool hasChanges = false;

            if (!string.IsNullOrEmpty(request.FullName) && request.FullName != user.FullName)
            {
                user.FullName = request.FullName;
                hasChanges = true;
            }

            if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
            {
                user.IsActive = request.IsActive.Value;
                hasChanges = true;
            }

            if (request.RoleId.HasValue && request.RoleId.Value != user.RoleId)
            {
                var roleExists = await _context.Roles
                    .AnyAsync(r => r.RoleId == request.RoleId.Value);
                
                if (!roleExists)
                {
                    return ServiceResult<UserDto>.ErrorResult("Invalid role specified");
                }
                user.RoleId = request.RoleId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                // Reload user with role for DTO mapping
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching user with ID: {UserId}, Request: {@Request}", id, request);
            return ServiceResult<UserDto>.ErrorResult($"An error occurred during user patch: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ActivateUserAsync(Guid id, bool isActive)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return ServiceResult<bool>.ErrorResult("User not found");
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating/deactivating user with ID: {UserId}, IsActive: {IsActive}", id, isActive);
            return ServiceResult<bool>.ErrorResult($"An error occurred during user activation: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return ServiceResult<bool>.ErrorResult("User not found");
            }

            // Check if user is referenced by projects or other entities
            var hasProjects = await _context.Projects
                .AnyAsync(p => p.ProjectManagerId == id);

            if (hasProjects)
            {
                return ServiceResult<bool>.ErrorResult("Cannot delete user: User is assigned as project manager to one or more projects");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return ServiceResult<bool>.ErrorResult($"An error occurred during user deletion: {ex.Message}");
        }
    }
}
