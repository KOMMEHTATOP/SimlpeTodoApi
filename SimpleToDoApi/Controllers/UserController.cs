using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.DTO.User.HelpersClassToService;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllUsers(
            [FromQuery] UserQueryParameters userQueryParameters)
        {
            var result = await _userService.GetAllUsersAsync(userQueryParameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto)
        {
            var result = await _userService.CreateAsync(createUserDto);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetUserById), new
                {
                    id = result.User!.Id
                }, result.User);
            }

            return BadRequest(new
            {
                Message = "User creation failed.", Errors = result.Errors
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            var result = await _userService.UpdateAsync(id, updateUserDto);

            if (result.Succeeded && result.User != null)
            {
                return Ok(result.User);
            }

            return BadRequest(new { Message = "User update failed", Errors = result.Errors });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var result = await _userService.DeleteAsync(id);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "User deletion failed.", Errors = result.Errors });
            }
            return NoContent();
        }
    }
}
