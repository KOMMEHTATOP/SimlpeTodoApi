using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers;

[Route("api/roles")]
[ApiController]
public class RoleController(ITodoContext context, IDatabaseCleaner databaseCleaner) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto roleDto)
    {
        if (await context.Roles.AnyAsync(r => r.Name == roleDto.Name))
        {
            return BadRequest("A role with this name already exists.");
        }

        var newRole = RoleMapper.FromDto(roleDto);

        context.Roles.Add(newRole);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoleById), new { id = newRole.Id }, RoleMapper.ToDto(newRole));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(int id)
    {
        var role = await context.Roles.FindAsync(id);

        if (role == null)
        {
            return NotFound("Role not found.");
        }

        return Ok(RoleMapper.ToDto(role));
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var rolesList = await context.Roles.ToListAsync();
        var roles = rolesList.Select(RoleMapper.ToDto).ToList();
        return Ok(roles);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole([FromRoute] int id, [FromBody] UpdateRoleDto roleDto)
    {
        var existingRole = await context.Roles.FindAsync(id);

        if (existingRole == null)
        {
            return NotFound("Role not found.");
        }

        if (existingRole.Name != roleDto.Name)
        {
            var nameConflict = await context.Roles.AnyAsync(r => r.Name == roleDto.Name && r.Id != id);

            if (nameConflict)
            {
                return BadRequest("A role with this name already exists.");
            }
        }

        existingRole.Name = roleDto.Name;
        existingRole.Description = roleDto.Description;

        await context.SaveChangesAsync();

        return Ok(RoleMapper.ToDto(existingRole));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound("Role not found.");
        }

        context.Roles.Remove(role);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAllRoles()
    {
        await databaseCleaner.ClearRoles();
        return NoContent();
    }
}