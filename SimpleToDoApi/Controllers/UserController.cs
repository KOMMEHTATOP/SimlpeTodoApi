using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static List<UserDTO> Users = new List<UserDTO>();
        private readonly TodoContext _context;

        public UserController(TodoContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddUser([FromBody] UserDTO user)
        {
            if (Users.Any(u => u.UserName == user.UserName))
            {
                return BadRequest("User already exists.");
            }
            Users.Add(user);
            return CreatedAtAction(nameof(AddUser), new { username = user.UserName }, user);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(Users);
        }

        [HttpPut("{id}")]
        public IActionResult PutUser(int id, UserDTO updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updatedUser.id)
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

    }
}
