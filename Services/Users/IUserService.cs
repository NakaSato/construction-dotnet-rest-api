
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Users;

public interface IUserService
{
    System.Threading.Tasks.Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string? role);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> GetUserByUsernameAsync(string username);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> PatchUserAsync(Guid id, PatchUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> ActivateUserAsync(Guid id, bool isActive);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteUserAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
}

