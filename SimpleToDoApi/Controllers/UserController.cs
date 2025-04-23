using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly DatabaseCleaner _databaseCleaner;

        public UserController(TodoContext context, DatabaseCleaner databaseCleaner)
        {
            _context = context;
            _databaseCleaner = databaseCleaner;
        }

        [HttpPost("add-new-user")]
        public IActionResult AddUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.UserName == createUserDto.UserName))
            {
                return BadRequest("User already exists.");
            }

            // 1. Получаем роли из БД
            var roles = _context.Roles
                .Where(r => createUserDto.RoleIds.Contains(r.Id))
                .ToList();

            // 2. Проверяем, все ли роли найдены
            var notFoundRoleIds = createUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();

            if (notFoundRoleIds.Any())
            {
                return BadRequest($"Не найдены роли с id: {string.Join(", ", notFoundRoleIds)}");
            }

            var passwordHash = createUserDto.Password;

            // 4. Маппим DTO → User (без ролей и пароля)
            var user = UserMapper.FromDto(createUserDto);

            // 5. Заполняем недостающие данные
            user.PasswordHash = passwordHash;
            user.Roles = roles;

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "База не доступна " + ex.Message);
            }

            return CreatedAtAction(nameof(GetUser), new
            {
                id = user.Id
            }, UserMapper.ToDto(user));
        }

        [HttpGet("get-all-users")]
        public IActionResult GetUsers()
        {
            var users = _context.Users
                .Include(u => u.Roles)
                .ToList();
            var usersDto = users.Select(u => UserMapper.ToDto(u)).ToList();
            return Ok(usersDto);
        }

        [HttpGet("get-user/{id}")]
        public ActionResult GetUser(int id)
        {
            var user = _context.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(UserMapper.ToDto(user));
        }


        [HttpPut("update-user/{id}")]
        public IActionResult PutUser([FromRoute] int id, UpdateUserDto updatedUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //находим пользователя в бд и разворачиваем его роли (include)
            var existingUser = _context.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Id == id);

            if (existingUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (_context.Users.Any(u => u.UserName == updatedUserDto.UserName && u.Id != id))
            {
                return BadRequest("Пользователь с таким именем уже существует.");
            }

            existingUser.UserName = updatedUserDto.UserName;

            //получаем из базы роли по id из DTO
            var roles = _context.Roles.Where(r => updatedUserDto.RoleIds.Contains(r.Id)).ToList();
            //Проверяем все ли роли найдены
            var notFoundRoleIds = updatedUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();

            if (notFoundRoleIds.Any())
            {
                return BadRequest($"Не найдены роли с id: {string.Join(", ", notFoundRoleIds)}");
            }

            if (roles.Count == 0)
            {
                return BadRequest("Пользователь должен иметь хотя бы одну роль.");
            }

            existingUser.Roles = roles;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return StatusCode(500, "База данных не доступна " + e.Message);
            }

            return Ok(UserMapper.ToDto(existingUser));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }

        // Удалить всех пользователей
        [HttpDelete("delete-all-users")]
        public IActionResult DeleteAllUsers()
        {
            _databaseCleaner.ClearUsers();
            return Ok("Все пользователи были удалены.");
        }
    }
}
