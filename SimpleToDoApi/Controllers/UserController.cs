using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var result = await userService.GetAllUsers();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var result = await userService.GetUserById(id);

            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var result = await userService.CreateAsync(createUserDto);

            if (result.Error != null)
            {
                switch (result.Error)
                {
                    case CreateUserResult.CreateUserError.UserNameExists:
                        return Conflict("User name already exists");
                    case CreateUserResult.CreateUserError.EmailExists:
                        return Conflict("Email already exists");
                    case CreateUserResult.CreateUserError.RoleNotFound:
                        return BadRequest("One or more roles not found");
                }
            }

            if (result.User == null)
            {
                return BadRequest("Ошибка создания пользователя");
            }

            return CreatedAtAction(nameof(GetUserById), new { id = result.User.Id }, result.User);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updatedUserDto)
        {
            var result = await userService.UpdateAsync(id, updatedUserDto);

            if (result.Error != null)
            {
                switch (result.Error)
                {
                    case UpdateUserResult.UpdateUserError.UserNameExists:
                        return Conflict("User name already exists");
                    case UpdateUserResult.UpdateUserError.RoleNotFound:
                        return BadRequest("One or more roles not found");
                    case UpdateUserResult.UpdateUserError.UserEmailExists:
                        return Conflict("Email already exists");
                    case UpdateUserResult.UpdateUserError.UserNotFound:
                        return NotFound("One or more users not found");
                    case UpdateUserResult.UpdateUserError.NoRolesProvided:
                        return BadRequest("No roles provided");
                }
            }
            
            return Ok(result.User);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await userService.DeleteAsync(id);

            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAllUsers()
        {
            await userService.DeleteAllAsync();
            return NoContent();
        }
    }
}
