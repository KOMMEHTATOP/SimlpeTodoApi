using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;
using SimpleToDoApi.Services;

namespace SimpleToDoApi.Controllers;

[Route("api/roles")]
[ApiController]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var rolesList = await roleService.GetAllRoles();
        return Ok(rolesList);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(int id)
    {
        var result = await roleService.GetRole(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto roleDto)
    {
        var result = await roleService.CreateRole(roleDto);

        if (result.Error == RoleResult.RoleError.RoleExists)
        {
            return Conflict("Role already exists");
        }

        if (result.Role == null)
        {
            return StatusCode(500, "Unexpected error: Role is null");
        }
        
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Role.Id }, result.Role);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole([FromRoute] int id, [FromBody] UpdateRoleDto roleDto)
    {
        var result = await roleService.UpdateRole(id, roleDto);

        if (result.Error != null)
        {
            switch (result.Error)
            {
                case RoleResult.RoleError.RoleExists:
                    return Conflict(result.Error);
                case RoleResult.RoleError.RoleNotFound:
                    return NotFound(result.Error);
            }
        }
        
        return Ok(result.Role);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        var result = await roleService.DeleteRole(id);

        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAllRoles()
    {
        await roleService.DeleteAllRoles();
        return NoContent();
    }
}
