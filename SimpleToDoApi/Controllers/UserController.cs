using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.Data;
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
        public IActionResult AddUser([FromBody] User user)
        {
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                return BadRequest("User already exists.");
            }

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                return StatusCode(500, "База не доступна " + ex.Message);
            }
            return CreatedAtAction(nameof(AddUser), new { username = user.UserName }, user);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return user;
        }


        [HttpPut("{id}")]
        public IActionResult PutUser(int id, User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updatedUser.Id)
            {
                return BadRequest("ID в URL и теле запроса не совпадают");
            }

            var existingUser = _context.Users.Find(id);
            if (existingUser == null)
            {
                return NotFound("Пользователя с таким ID не существует");
            }

            existingUser.UserName = updatedUser.UserName;
            existingUser.Role = updatedUser.Role;

            try
            {
                _context.SaveChanges();

            }
            catch (Exception ex)
            {

                return StatusCode(500, "База не доступна " + ex.Message);
            }

            return Ok(existingUser);
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
