using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;
using System.Runtime.InteropServices.JavaScript;

namespace SimpleToDoApi.Services;

public class RoleService(IDatabaseCleaner databaseCleaner, ITodoContext context) : IRoleService
{
    private readonly IDatabaseCleaner _databaseCleaner = databaseCleaner;
    private readonly ITodoContext _context = context;

    public async Task<List<RoleDto>> GetAllRoles()
    {
        var rolesList = await context.Roles.ToListAsync();
        return rolesList.Select(RoleMapper.ToDto).ToList();
    }

    public async Task<RoleDto?> GetRole(int idRole)
    {
        var role = await context.Roles.FindAsync(idRole);

        if (role == null)
        {
            return null;
        }

        return RoleMapper.ToDto(role);
    }

    public async Task<RoleResult> CreateRole(CreateRoleDto roleDto)
    {
        bool roleExists = await context.Roles.AnyAsync(r => r.Name == roleDto.Name);

        if (roleExists)
        {
            return new RoleResult
            {
                Error = RoleResult.RoleError.RoleExists
            };
        }

        var newRole = RoleMapper.FromDto(roleDto);

        context.Roles.Add(newRole);
        await context.SaveChangesAsync();

        return new RoleResult()
        {
            Role = RoleMapper.ToDto(newRole)
        };
    }

    public async Task<RoleResult> UpdateRole(int idRole, UpdateRoleDto roleDto)
    {
        var existingRole = await context.Roles.FindAsync(idRole);

        if (existingRole == null)
        {
            return new RoleResult()
            {
                Error = RoleResult.RoleError.RoleNotFound
            };
        }

        if (existingRole.Name != roleDto.Name)
        {
            var nameConflict = await context.Roles.AnyAsync(r => r.Name == roleDto.Name && r.Id != idRole);

            if (nameConflict)
            {
                return new RoleResult()
                {
                    Error = RoleResult.RoleError.RoleExists
                };
            }
        }

        existingRole.Name = roleDto.Name;
        existingRole.Description = roleDto.Description;

        await context.SaveChangesAsync();

        return new RoleResult()
        {
            Role = RoleMapper.ToDto(existingRole)
        };
    }
    
    public async Task<bool> DeleteRole(int idRole)
    {
        var result = await context.Roles.FindAsync(idRole);

        if (result == null)
        {
            return false;
        }
        context.Roles.Remove(result);
        await context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> DeleteAllRoles()
    {
        await databaseCleaner.ClearRoles();
        return true;

    }
}
