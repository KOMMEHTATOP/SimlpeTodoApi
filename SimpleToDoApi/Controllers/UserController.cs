using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(TodoContext context, DatabaseCleaner databaseCleaner) : ControllerBase
    {
        [HttpPost("add-new-user")]
        public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto createUserDto)
        {
            // Асинхронно проверяем, существует ли уже пользователь с таким именем (UserName)
            // AnyAsync выполняет SQL-запрос SELECT EXISTS(SELECT 1 ...) к базе данных
            if (await context.Users.AnyAsync(u => u.UserName == createUserDto.UserName))
            {
                return BadRequest("User already exists.");
            }

            // Асинхронно получаем из базы данных все роли, id которых были переданы клиентом
            // ToListAsync вернёт только те роли, которые реально есть в базе
            var roles = await context.Roles
                .Where(r => createUserDto.RoleIds.Contains(r.Id))
                .ToListAsync();

            // Определяем, есть ли среди запрошенных id такие, которых нет в базе
            // createUserDto.RoleIds - все id, которые прислал клиент
            // roles.Select(r => r.Id) - id реально существующих ролей из базы
            // Except вернёт те id, которых не найдено
            var notFoundRoleIds = createUserDto.RoleIds.Except(roles.Select(r => r.Id)).ToList();

            // Если есть несуществующие id ролей — возвращаем ошибку с их перечислением
            if (notFoundRoleIds.Any())
            {
                return BadRequest($"Не найдены роли с id: {string.Join(", ", notFoundRoleIds)}");
            }

            // Здесь должен быть хэш пароля, но сейчас просто присваиваем как есть (НЕ БЕЗОПАСНО!)
            var passwordHash = createUserDto.Password;

            // Маппим DTO → User (создаём новый объект User на основе входных данных, кроме ролей и пароля)
            var user = UserMapper.FromDto(createUserDto);

            // Заполняем недостающие поля: пароль и роли
            user.PasswordHash = passwordHash;
            user.Roles = roles;

            try
            {
                // Добавляем нового пользователя в отслеживаемый список (ещё не в БД!)
                context.Users.Add(user);

                // Асинхронно сохраняем изменения в базе данных (INSERT в таблицу Users и связи с ролями)
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Если произошла ошибка при сохранении — возвращаем 500 и сообщение об ошибке
                return StatusCode(500, "База не доступна " + ex.Message);
            }

            // Возвращаем статус 201 Created и данные нового пользователя
            // CreatedAtAction позволяет клиенту узнать URL для получения информации о созданном пользователе
            return CreatedAtAction(nameof(GetUser), new
            {
                id = user.Id
            }, UserMapper.ToDto(user));
        }

        [HttpGet("get-all-users")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            // Асинхронно загружаем всех пользователей из базы данных, включая их роли (JOIN через Include)
            var users = await context.Users
                .Include(u => u.Roles) // Жадная загрузка ролей (navigation property)
                .ToListAsync(); // Асинхронно получаем список пользователей

            // Маппим пользователей из сущностей User в DTO (UserDto), чтобы не светить внутреннюю структуру БД наружу
            var usersDto = users
                .Select(UserMapper.ToDto)
                .ToList();

            // Возвращаем HTTP 200 OK и список пользователей-DTO
            return Ok(usersDto);
        }

        [HttpGet("get-user/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(UserMapper.ToDto(user));
        }

        [HttpPut("update-user/{id}")]
        public async Task<ActionResult<UserDto>> PutUser([FromRoute] int id, [FromBody] UpdateUserDto updatedUserDto)
        {
            //находим пользователя в бд и разворачиваем его роли (include)
            var existingUser = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (await context.Users.AnyAsync(u => u.UserName == updatedUserDto.UserName && u.Id != id))
            {
                return BadRequest("Пользователь с таким именем уже существует.");
            }

            existingUser.UserName = updatedUserDto.UserName;

            //получаем из базы роли по id 
            var roles = await context.Roles
                .Where(r => updatedUserDto.RoleIds.Contains(r.Id))
                .ToListAsync();

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
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, "База данных не доступна " + e.Message);
            }

            return Ok(UserMapper.ToDto(existingUser));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            context.Users.Remove(user);

            try
            {
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, "Ошибка при удалении задачи" + e.Message);
            }
        }
        
        [HttpDelete("delete-all-users")]
        public async Task<ActionResult> DeleteAllUsers()
        {
            try
            {
                await databaseCleaner.ClearUsers();
                return NoContent();
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, "Ошибка удаленния данных из БД" + e.Message);
            }
        }
    }
}
