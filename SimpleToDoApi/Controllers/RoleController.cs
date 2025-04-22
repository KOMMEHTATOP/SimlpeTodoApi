using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers;

public class RoleController : Controller
{
    private readonly ITodoContext _context;
    private readonly DatabaseCleaner _databaseCleaner;

    public RoleController(ITodoContext context, DatabaseCleaner databaseCleaner)
    {
        _context = context;
        _databaseCleaner = databaseCleaner;
    }

    [HttpPost("create-newrole")]
    public IActionResult CreateNewRole([FromBody] CreateRoleDto roleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (_context.Roles.Any(r => r.Name == roleDto.Name))
        {
            return BadRequest("Роль уже существует");
        }

        var newRole = RoleMapper.FromDto(roleDto);

        try
        {
            _context.Roles.Add(newRole);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            return StatusCode(500, "База не доступна " + e.Message);
        }

        return CreatedAtAction(nameof(GetRole), new
        {
            id = newRole.Id
        }, RoleMapper.ToDto(newRole));
    }

    [HttpGet("role/{id}")]
    public IActionResult GetRole(int id)
    {
        var role = _context.Roles.FirstOrDefault(r => r.Id == id);

        if (role == null)
        {
            return NotFound("Роль не найдена");
        }

        return Ok(RoleMapper.ToDto(role));
    }

    // GET
    [HttpGet("view-all-roles")]
    public IActionResult GetAllRoles()
    {
        var roles = _context.Roles
            .Select(RoleMapper.ToDto)
            .ToList();
        return Ok(roles);
    }

    [HttpPut("update-role/{id}")]
    public IActionResult UpdateRole([FromRoute] int id, [FromBody] UpdateRoleDto roleDto)
    {
        var existingRole = _context.Roles.FirstOrDefault(r => r.Id == id);

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

            var oldNameRole = _context.Roles.Any(r => r.Name == roleDto.Name && r.Id != id);

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
            _context.SaveChanges();
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
        var role = _context.Roles.FirstOrDefault(r => r.Id == id);

        if (role == null)
        {
            return NotFound("Роль для удаления не найдена.");
        }

        try
        {
            _context.Roles.Remove(role);
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
        _databaseCleaner.ClearRoles();
        return NoContent();
    }
}
