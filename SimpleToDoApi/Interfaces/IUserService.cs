using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;

namespace SimpleToDoApi.Interfaces;

public interface IUserService
{
    Task<UserPagedResult<UserDto>> GetAllUsers(UserQueryParameters userQueryParameters);
    Task<UserDto?> GetUserById(string id);
    Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto);
    Task<UpdateUserResult> UpdateAsync(UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteAllAsync();
}
