using Microsoft.AspNetCore.Identity;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User.HelpersClassToService; 
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Services;

public class UserService : IUserService
{
    private readonly ITodoContext _context;
    private readonly IDatabaseCleaner _databaseCleaner;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(
        ITodoContext context, 
        IDatabaseCleaner databaseCleaner, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _databaseCleaner = databaseCleaner;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    public Task<PagedResult<UserDto>> GetAllUsersAsync(UserQueryParameters userQueryParameters)
    {
        throw new NotImplementedException();
    }
    public Task<UserDto?> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto)
    {
        throw new NotImplementedException("Метод CreateAsync будет реализован с ASP.NET Core Identity.");
    }

    public Task<UpdateUserResult> UpdateAsync(string id, UpdateUserDto updateUserDto)
    {
        throw new NotImplementedException("Метод UpdateAsync будет реализован позже с ASP.NET Core Identity.");
    }

    public Task<DeleteUserResult> DeleteAsync(string id)
    {
        throw new NotImplementedException("Метод DeleteAsync будет реализован позже с ASP.NET Core Identity.");
    }
}