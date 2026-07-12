
using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Users;

public interface IUserService
{
    System.Threading.Tasks.Task<Result<PagedResult<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string? role);
    System.Threading.Tasks.Task<Result<UserDto>> GetUserByIdAsync(Guid id);
    System.Threading.Tasks.Task<Result<UserDto>> GetUserByUsernameAsync(string username);
    System.Threading.Tasks.Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request);
    System.Threading.Tasks.Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    System.Threading.Tasks.Task<Result<UserDto>> PatchUserAsync(Guid id, PatchUserRequest request);
    System.Threading.Tasks.Task<Result<bool>> ActivateUserAsync(Guid id, bool isActive);
    System.Threading.Tasks.Task<Result<bool>> DeleteUserAsync(Guid id);
    System.Threading.Tasks.Task<Result<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
}

