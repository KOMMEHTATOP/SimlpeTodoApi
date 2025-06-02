using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.DTO.User.HelpersClassToService;
using SimpleToDoApi.DTO.User.ResultClassesUsers;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITodoContext _context;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ITodoContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<PagedResult<UserDto>> GetAllUsersAsync(UserQueryParameters userQueryParameters)
    {
        var query = _userManager.Users;

        var resultPage = new PagedResult<UserDto>();

        resultPage.PageNumber = userQueryParameters.PageNumber;
        resultPage.PageSize = userQueryParameters.PageSize;
        resultPage.TotalCount = await query.CountAsync();

        var currentPageUsersQuery = query.Skip((userQueryParameters.PageNumber - 1) * userQueryParameters.PageSize);
        var usersForPageQuery = currentPageUsersQuery.Take(userQueryParameters.PageSize);
        var usersOnPage = await usersForPageQuery.ToListAsync();
        var idUsers = usersOnPage.Select(x => x.Id).ToList();
        var userRoleLinks = await _context.UserRoles
            .Where(x => idUsers.Contains(x.UserId))
            .ToListAsync();
        var roleIds = userRoleLinks.Select(ur => ur.RoleId)
            .Distinct()
            .ToList();
        var roles = await _roleManager.Roles
            .Where(x => roleIds.Contains(x.Id))
            .ToListAsync();

        var roleDict = roles.ToDictionary(r => r.Id, r => r.Name);
        var groupedByUser = userRoleLinks.GroupBy(x => x.UserId);
        
        var rolesByUser = new Dictionary<string, List<string>>();

        foreach (var group in groupedByUser)
        {
            var userId = group.Key;
            var userRoleNames = new List<string>();

            foreach (var userRole in group)
            {
                if (roleDict.TryGetValue(userRole.RoleId, out var roleName) && roleName != null)
                {
                    userRoleNames.Add(roleName); 
                }
            }
    
            rolesByUser[userId] = userRoleNames;
        }
        
        List<UserDto> usersDtoForPage = new List<UserDto>();

        foreach (var user in usersOnPage)
        {
            if (!rolesByUser.TryGetValue(user.Id, out var userRoles))
            {
                userRoles = new List<string>(); 
            }
        
            var preparedUser = UserMapper.ToDto(user, userRoles);
            usersDtoForPage.Add(preparedUser);
        }

        resultPage.Items = usersDtoForPage;
        return resultPage;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var userAssign = await _userManager.FindByIdAsync(id);

        if (userAssign == null)
        {
            return null;
        }

        var userRoles = await _userManager.GetRolesAsync(userAssign);

        return UserMapper.ToDto(userAssign, userRoles);
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto)
    {
        var applicationUser = UserMapper.ToApplicationUser(createUserDto);
        var identityResult = await _userManager.CreateAsync(applicationUser, createUserDto.Password);

        if (identityResult.Succeeded)
        {
            string defaultRoleName = "User";
            bool roleExists = await _roleManager.RoleExistsAsync(defaultRoleName);

            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new ApplicationRole(defaultRoleName));

                if (!roleResult.Succeeded)
                {
                    return CreateUserResult.Failed(roleResult.Errors);
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(applicationUser, defaultRoleName);

            if (!addToRoleResult.Succeeded)
            {
                return CreateUserResult.Failed(addToRoleResult.Errors);
            }

            var assignedRole = await _userManager.GetRolesAsync(applicationUser);

            var userToDto = UserMapper.ToDto(applicationUser, assignedRole);
            return new CreateUserResult(userToDto);
        }

        return CreateUserResult.Failed(identityResult.Errors);
    }

    public async Task<UpdateUserResult> UpdateAsync(string id, UpdateUserDto updateUserDto)
    {
        var userToUpdate = await _userManager.FindByIdAsync(id);

        if (userToUpdate == null)
        {
            return UpdateUserResult.Failed("User not found");
        }

        if (userToUpdate.UserName != updateUserDto.UserName && !string.IsNullOrWhiteSpace(updateUserDto.UserName))
        {
            var newUserName = await _userManager.SetUserNameAsync(userToUpdate, updateUserDto.UserName);

            if (!newUserName.Succeeded)
            {
                return UpdateUserResult.Failed(newUserName.Errors);
            }
        }

        if (userToUpdate.Email != updateUserDto.Email && !string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            var newUserEmail = await _userManager.SetEmailAsync(userToUpdate, updateUserDto.Email);

            if (!newUserEmail.Succeeded)
            {
                return UpdateUserResult.Failed(newUserEmail.Errors);
            }
        }

        var userRoles = await _userManager.GetRolesAsync(userToUpdate);
        var toRemoveRoles = userRoles.Except(updateUserDto.RoleNames).ToList();
        var toAddRoles = updateUserDto.RoleNames.Except(userRoles).ToList();

        if (toRemoveRoles.Count != 0 || toAddRoles.Count != 0)
        {
            if (toRemoveRoles.Count > 0)
            {
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(userToUpdate, toRemoveRoles);

                if (!removeRoleResult.Succeeded)
                {
                    return UpdateUserResult.Failed(removeRoleResult.Errors);
                }
            }

            if (toAddRoles.Count > 0)
            {
                foreach (var roleName in toAddRoles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);

                    if (!roleExists)
                    {
                        return UpdateUserResult.Failed($"Role '{roleName}' does not exist and cannot be assigned.");
                    }    
                }
                
                var addRoleResult = await _userManager.AddToRolesAsync(userToUpdate, toAddRoles);

                if (!addRoleResult.Succeeded)
                {
                    return UpdateUserResult.Failed(addRoleResult.Errors);
                }
            }
        }

        
        var updatedRoles = await _userManager.GetRolesAsync(userToUpdate);
        
        if (updatedRoles.Count == 0)
        {
            return UpdateUserResult.Failed("User must have at least one role");
        }
        
        var updatedUserDto = UserMapper.ToDto(userToUpdate, updatedRoles);
        return new UpdateUserResult(updatedUserDto);
    }

    public async Task<DeleteUserResult> DeleteAsync(string id)
    {
        var userToDelete = await _userManager.FindByIdAsync(id);

        if (userToDelete == null)
        {
            return DeleteUserResult.Failed("User not found");
        }
        
        var deleteResult = await _userManager.DeleteAsync(userToDelete);

        if (!deleteResult.Succeeded)
        {
            return DeleteUserResult.Failed(deleteResult.Errors);
        }

        return DeleteUserResult.Success();
    }
}
