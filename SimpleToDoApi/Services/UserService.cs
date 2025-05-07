using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;
using System.Runtime.InteropServices.JavaScript;

namespace SimpleToDoApi.Services;

public class UserService : IUserService
{

    private readonly ITodoContext _context;
    private readonly IDatabaseCleaner _databaseCleaner;

    public UserService(ITodoContext context, IDatabaseCleaner databaseCleaner)
    {
        _context = context;
        _databaseCleaner = databaseCleaner;
    }
    
    public async Task<List<UserDto>> GetAllUsers()
    {
        var users = await _context.Users
            .Include(u => u.Roles)
            .ToListAsync();
        return users.Select(UserMapper.ToDto).ToList();
    }
    
    public async Task<UserDto?> GetUserById(int id)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);
        return UserMapper.ToDto(user);
    }
    
    public async Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto)
    {
        bool userExists = await _context.Users.AnyAsync(u => u.UserName == createUserDto.UserName);
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == createUserDto.Email);

        if (userExists)
        {
            return new CreateUserResult
            {
                Error = CreateUserResult.CreateUserError.UserNameExists
            };
        }
        
        if (emailExists)
        {
            return new CreateUserResult
            {
                Error = CreateUserResult.CreateUserError.EmailExists
            };
        }

        var roles = await _context.Roles
            .Where(r => createUserDto.RoleIds.Contains(r.Id))
            .ToListAsync();
        
        var notFoundRoleIds = createUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();
        
        if (notFoundRoleIds.Any())
        {
            return new CreateUserResult
            {
                Error = CreateUserResult.CreateUserError.RoleNotFound
            };
        }
        
        var passwordHash = createUserDto.Password;
        var user = UserMapper.FromDto(createUserDto);
        user.PasswordHash = passwordHash;
        user.Roles = roles;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new CreateUserResult
        {
            User = UserMapper.ToDto(user)
        };
    }
    
    public async Task<UpdateUserResult> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        var existingUser = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (existingUser == null)
        {
            return new UpdateUserResult
            {
                Error = UpdateUserResult.UpdateUserError.UserNotFound
            };
        }

        bool userExists = await _context.Users.AnyAsync(u => u.UserName == updateUserDto.UserName && u.Id != existingUser.Id);
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != existingUser.Id);
        if (userExists)
        {
            return new UpdateUserResult
            {
                Error = UpdateUserResult.UpdateUserError.UserNameExists
            };
        }
        
        if (emailExists)
        {
            return new UpdateUserResult
            {
                Error = UpdateUserResult.UpdateUserError.UserEmailExists
            };
        }

        var roles = await _context.Roles
            .Where(r => updateUserDto.RoleIds.Contains(r.Id))
            .ToListAsync();
        var notFoundRoleIds = updateUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();

        if (notFoundRoleIds.Any())
        {
            return new UpdateUserResult
            {
                Error = UpdateUserResult.UpdateUserError.RoleNotFound
            };
        }

        if (!roles.Any())
        {
            return new UpdateUserResult
            {
                Error = UpdateUserResult.UpdateUserError.NoRolesProvided
            };
        }
        
        existingUser.UserName = updateUserDto.UserName;
        existingUser.Email = updateUserDto.Email;
        existingUser.Roles = roles;
        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();

        return new UpdateUserResult
        {
            User = UserMapper.ToDto(existingUser)
        };
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteAllAsync()
    {
        await _databaseCleaner.ClearUsers();
        return true;
    }
}
