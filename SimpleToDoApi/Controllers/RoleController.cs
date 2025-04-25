using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers;

[ApiController]
public class RoleController(ITodoContext context, DatabaseCleaner databaseCleaner) : Controller
{
    [HttpPost("create-newrole")]
    public async Task<ActionResult<RoleDto>> CreateNewRole([FromBody] CreateRoleDto roleDto)
    {
        if (await context.Roles.AnyAsync(r => r.Name == roleDto.Name))
        {
            return BadRequest("Роль уже существует");
        }

        var newRole = RoleMapper.FromDto(roleDto);

        context.Roles.Add(newRole);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "База не доступна " + e.Message);
        }

        return CreatedAtAction(nameof(GetRole), new
        {
            id = newRole.Id
        }, RoleMapper.ToDto(newRole));
    }

    [HttpGet("role/{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        var role = await context.Roles.FindAsync(id);

        if (role == null)
        {
            return NotFound("Роль базе данных не найдена");
        }

        return Ok(RoleMapper.ToDto(role));
    }

    [HttpGet("view-all-roles")]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var roles = await context.Roles
            .Select(RoleMapper.ToDto)
            .AsQueryable()
            .ToListAsync();
        return Ok(roles);
    }

    [HttpPut("update-role/{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole([FromRoute] int id, [FromBody] UpdateRoleDto roleDto)
    {
        var existingRole = await context.Roles.FindAsync(id);

        if (existingRole == null)
        {
            return NotFound("Роль не найдена!");
        }

        if (existingRole.Name != roleDto.Name)
        {
            var oldNameRole = await context.Roles.AnyAsync(r => r.Name == roleDto.Name && r.Id != id);

            if (oldNameRole)
            {
                return BadRequest("Роль с таким именем уже существует!");
            }
        }

        existingRole.Name = roleDto.Name;
        existingRole.Description = roleDto.Description;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "Ошибка при сохранении данных в базе" + e.Message);
        }

        return Ok(RoleMapper.ToDto(existingRole));
    }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound("Роль для удаления не найдена.");
        }

        context.Roles.Remove(role);

        try
        {
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Ошибка при обращении в базе данных" + e.Message);
        }
    }

    [HttpDelete("delete-all-role")]
    public async Task<ActionResult> DeleteAllRoles()
    {
        try
        {
            await databaseCleaner.ClearRoles();
            return Ok("Все роли были удалены!");

        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "База данных не доступна: " + e.Message);
        }
    }
}
