using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.DTO.User.HelpersClassToService;
using SimpleToDoApi.DTO.User.ResultClassesUsers;

namespace SimpleToDoApi.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllUsersAsync(UserQueryParameters userQueryParameters);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto);
    Task<UpdateUserResult> UpdateAsync(string id, UpdateUserDto updateUserDto);
    Task<DeleteUserResult> DeleteAsync(string id);
}
