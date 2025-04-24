using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers;

public class RoleController(ITodoContext context, DatabaseCleaner databaseCleaner) : Controller
{
    [HttpPost("create-newrole")]
    public async Task<ActionResult<RoleDto>> CreateNewRole([FromBody] CreateRoleDto roleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
    public IActionResult UpdateRole([FromRoute] int id, [FromBody] UpdateRoleDto roleDto)
    {
        var existingRole = context.Roles.Find(id);

        if (existingRole == null)
        {
            return NotFound("Роль не найдена!");
        }

        if (existingRole.Name != roleDto.Name)
        {
            if (string.IsNullOrEmpty(roleDto.Name))
            {
                return BadRequest("Имя роли не может быть пустым!");
            }

            var oldNameRole = context.Roles.Any(r => r.Name == roleDto.Name && r.Id != id);

            if (oldNameRole)
            {
                return BadRequest("Роль с таким именем уже существует!");
            }

            existingRole.Name = roleDto.Name;
        }

        if (existingRole.Description != roleDto.Description)
        {
            existingRole.Description = roleDto.Description;
        }

        try
        {
            context.SaveChanges();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Ошибка при сохранении данных в базе" + e.Message);
        }

        return Ok(RoleMapper.ToDto(existingRole));
    }

    [HttpDelete("delete/{id}")]
    public IActionResult DeleteRole(int id)
    {
        var role = context.Roles.FirstOrDefault(r => r.Id == id);

        if (role == null)
        {
            return NotFound("Роль для удаления не найдена.");
        }

        try
        {
            context.Roles.Remove(role);
            context.SaveChanges();
            return NoContent();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Ошибка при обращении в базе данных" + e.Message);
        }
    }

    [HttpDelete("delete-all-role")]
    public IActionResult DeleteAllRoles()
    {
        databaseCleaner.ClearRoles();
        return Ok("Все роли были удалены!");
    }
}
