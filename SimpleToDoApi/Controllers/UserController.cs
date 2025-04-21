using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Mappers;
using SimpleToDoApi.Models;

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

        [HttpPost]
        public IActionResult AddUser([FromBody] CreateUserDTO createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (_context.Users.Any(u => u.UserName == createUserDto.UserName))
            {
                return BadRequest("User already exists.");
            }

            var roles = _context.Roles.Where(r=>createUserDto.RoleIds.Contains(r.Id)).ToList();
            var notFoundRoleIds = createUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();

            if (notFoundRoleIds.Any())
            {
                return BadRequest($"Не найдены роли с id: {string.Join(", ", notFoundRoleIds)}");
            }

            var passwordHash = createUserDto.Password;
            
            var user = UserMapper.FromDTO(createUserDto, roles, passwordHash);
                
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "База не доступна " + ex.Message);
            }
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, UserMapper.ToDTO(user));
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users.ToList();
            var usersDTO = users.Select(u => UserMapper.ToDTO(u)).ToList();
            return Ok(usersDTO);
        }

        [HttpGet("{id}")]
        public ActionResult GetUser(int id)
        {
            var user = _context.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }
            
            return Ok(UserMapper.ToDTO(user));
        }


        [HttpPut("{id}")]
        public IActionResult PutUser(int id, UpdateUserDTO updatedUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //находим пользователя в бд
            var existingUser = _context.Users
                .Include(u=>u.Roles)
                .FirstOrDefault(u=>u.Id == id);

            if (existingUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (_context.Users.Any(u=>u.UserName == updatedUserDTO.UserName && u.Id != id))
            {
                return BadRequest("Пользователь с таким именем уже существует.");
            }
            existingUser.UserName = updatedUserDTO.UserName;
            
            //получаем из базы роли по id из DTO
            var roles = _context.Roles.Where(r=>updatedUserDTO.RoleIds.Contains(r.Id)).ToList();
            //Проверяем все ли роли найдены
            var notFoundRoleIds = updatedUserDTO.RoleIds.Except(roles.Select(r => r.Id)).ToList();

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
            return Ok(UserMapper.ToDTO(existingUser));
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
