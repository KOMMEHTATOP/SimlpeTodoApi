using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController(ITodoContext context, IDatabaseCleaner databaseCleaner) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (await context.Users.AnyAsync(u => u.UserName == createUserDto.UserName))
                return BadRequest("A user with this username already exists.");

            var roles = await context.Roles
                .Where(r => createUserDto.RoleIds.Contains(r.Id))
                .ToListAsync();

            var notFoundRoleIds = createUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();
            if (notFoundRoleIds.Any())
                return BadRequest($"Roles not found with id(s): {string.Join(", ", notFoundRoleIds)}");

            if (await context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                return BadRequest("A user with this email already exists.");

            var passwordHash = createUserDto.Password;
            var user = UserMapper.FromDto(createUserDto);
            user.PasswordHash = passwordHash;
            user.Roles = roles;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, UserMapper.ToDto(user));
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await context.Users
                .Include(u => u.Roles)
                .ToListAsync();

            var usersDto = users.Select(UserMapper.ToDto).ToList();

            return Ok(usersDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found.");

            return Ok(UserMapper.ToDto(user));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updatedUserDto)
        {
            var existingUser = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
                return NotFound("User not found.");

            if (await context.Users.AnyAsync(u => u.UserName == updatedUserDto.UserName && u.Id != id))
                return BadRequest("A user with this username already exists.");

            existingUser.UserName = updatedUserDto.UserName;

            var roles = await context.Roles
                .Where(r => updatedUserDto.RoleIds.Contains(r.Id))
                .ToListAsync();

            var notFoundRoleIds = updatedUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();
            if (notFoundRoleIds.Any())
                return BadRequest($"Roles not found with id(s): {string.Join(", ", notFoundRoleIds)}");

            if (roles.Count == 0)
                return BadRequest("User must have at least one role.");

            if (await context.Users.AnyAsync(u => u.Email == updatedUserDto.Email && u.Id != id))
                return BadRequest("A user with this email already exists.");

            existingUser.Email = updatedUserDto.Email;
            existingUser.Roles = roles;

            await context.SaveChangesAsync();

            return Ok(UserMapper.ToDto(existingUser));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAllUsers()
        {
            await databaseCleaner.ClearUsers();
            return NoContent();
        }
    }
}