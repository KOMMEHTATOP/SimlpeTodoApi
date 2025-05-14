using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;

namespace SimpleToDoApi.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsers();
    Task<UserDto?> GetUserById(string id);
    Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto);
    Task<UpdateUserResult> UpdateAsync(UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteAllAsync();
}
