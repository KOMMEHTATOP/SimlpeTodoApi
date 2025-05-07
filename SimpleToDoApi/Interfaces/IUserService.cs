using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO;

namespace SimpleToDoApi.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsers();
    Task<UserDto?> GetUserById(int id);
    Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto);
    Task<UpdateUserResult> UpdateAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllAsync();
}
