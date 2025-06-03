using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.DTO.Role.ResultClassesRole;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var rolesList = await _roleService.GetAllRolesAsync();
        return Ok(rolesList);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(string id)
    {
        var roleDto = await _roleService.GetRoleAsync(id);

        if (roleDto == null)
        {
            return NotFound();
        }
        
        return Ok(roleDto);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto roleDto)
    {
        var result = await _roleService.CreateRoleAsync(roleDto);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Role!.Id }, result.Role);
    }
    

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole([FromRoute] string id, [FromBody] UpdateRoleDto roleDto)
    {
        var result = await _roleService.UpdateRoleAsync(id, roleDto);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        return Ok(result.Role);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(string id)
    {
        var result = await _roleService.DeleteRoleAsync(id);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        return NoContent();
    }
}
