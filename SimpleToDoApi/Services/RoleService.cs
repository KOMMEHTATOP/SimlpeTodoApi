using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.DTO.Role.ResultClassesRole;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Services;

public class RoleService : IRoleService 
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }
    
    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var rolesList = await _roleManager.Roles.ToListAsync();
        return rolesList.Select(r => new RoleDto { Id = r.Id, Name = r.Name, Description = r.Description}).ToList();
    }

    public async Task<RoleDto?> GetRoleAsync(string idRole)
    {
        var result = await _roleManager.FindByIdAsync(idRole);

        if (result == null)
        {
            return null;    
        }
        
        return new RoleDto { Id = result.Id, Name = result.Name, Description = result.Description };
    }

    public async Task<RoleResult> CreateRoleAsync(CreateRoleDto roleDto)
    {
        var newRole = new ApplicationRole(roleDto.Name!) {Description = roleDto.Description};
        var result = await _roleManager.CreateAsync(newRole);

        if (!result.Succeeded)
        {
            return RoleResult.Failed(result.Errors.Select(e => e.Description).ToList());
        }
        
        var roleDtoResult = new RoleDto { Id = newRole.Id, Name = newRole.Name, Description = newRole.Description };
        return RoleResult.Success(roleDtoResult);
    }

    public async Task<RoleResult> UpdateRoleAsync(string idRole, UpdateRoleDto roleDto)
    {
        var existingRole = await _roleManager.FindByIdAsync(idRole);
        if (existingRole == null)
        {
            return RoleResult.Failed("Role not found");
        }
        existingRole.Name = roleDto.Name;
        existingRole.Description = roleDto.Description;
        var result = await _roleManager.UpdateAsync(existingRole);

        if (!result.Succeeded)
        {
            return RoleResult.Failed(result.Errors.Select(e => e.Description).ToList());
        }
        
        var roleDtoResult = new RoleDto { Id = existingRole.Id, Name = existingRole.Name, Description = existingRole.Description };
        return RoleResult.Success(roleDtoResult);
    }

    public async Task<RoleResult> DeleteRoleAsync(string idRole)
    {
        var existingRole = await _roleManager.FindByIdAsync(idRole);

        if (existingRole == null)
        {
            return RoleResult.Failed("Role not found");
        }
        
        var result = await _roleManager.DeleteAsync(existingRole);

        if (!result.Succeeded)
        {
            return RoleResult.Failed(result.Errors.Select(e => e.Description).ToList());
        }
    
        return RoleResult.Success();
    }
}