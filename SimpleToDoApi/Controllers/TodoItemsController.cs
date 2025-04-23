using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoContext _context;
        private readonly DatabaseCleaner? _databaseCleaner;
        public TodoItemsController(ITodoContext context, DatabaseCleaner? databaseCleaner = null)
        {
            _context = context;
            _databaseCleaner = databaseCleaner;
        }

        [HttpPost("create-new-task")]
        public ActionResult PostTodoItem([FromBody] CreateToDoItemDto createTodoItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.ToDoItems.Any(t => t.Title == createTodoItemDto.Title))
            {
                return BadRequest(
                    "Задача с таким заголовком уже существует. Скоректируйте/удалите существующую или создайте задачу с новым заголовком.");
            }

            var newToDoItem = ToDoItemMapper.FromDto(createTodoItemDto);

            try
            {
                _context.ToDoItems.Add(newToDoItem);
                _context.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                var errorMessage = e.InnerException?.Message ?? e.Message;
                return StatusCode(500, $"Ошибка при сохранении: {errorMessage}");
            }

            return CreatedAtAction(nameof(GetTodoItem), new
            {
                id = newToDoItem.Id
            }, ToDoItemMapper.ToDto(newToDoItem));
        }

        // GET: api/TodoItems
        [HttpGet("view-all-tasks")]
        public ActionResult<List<ToDoItemDto>> GetTodoItems()
        {
            var tasks = _context.ToDoItems
                .Select(ToDoItemMapper.ToDto)
                .ToList();
            return Ok(tasks);
        }

        // GET: api/TodoItems/1
        [HttpGet("view-task-ID-{id}")]
        public ActionResult<ToDoItemDto> GetTodoItem(int id)
        {
            var todoItem = _context.ToDoItems.Find(id);

            if (todoItem == null)
            {
                return NotFound("Задача в базе данных не найдена");
            }

            return Ok(ToDoItemMapper.ToDto(todoItem));
        }


        // PUT: api/TodoItems/1
        [HttpPut("update-task-{id}")]
        public ActionResult<ToDoItemDto> UpdateTodoItem([FromRoute] int id, [FromBody] UpdateToDoItemDto newToDoItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.ToDoItems.Any(t => t.Title == newToDoItemDto.Title && t.Id != id))
            {
                return BadRequest("Задача с таким заголовком уже существует.");
            }
            
            var existingTodoItem = _context.ToDoItems.Find(id);
            
            if (existingTodoItem == null)
            {
                return NotFound("Задача не найдена!");
            }

            existingTodoItem.Title = newToDoItemDto.Title;
            existingTodoItem.Description = newToDoItemDto.Description;
            existingTodoItem.IsComplete = newToDoItemDto.IsComplete;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "База не доступна " + ex.Message);
            }

            return Ok(ToDoItemMapper.ToDto(existingTodoItem));
        }

        // DELETE: api/TodoItems/1
        [HttpDelete("delete-task-{id}")]
        public IActionResult DeleteTodoItem(int id)
        {
            var todoItem = _context.ToDoItems.Find(id);

            if (todoItem == null)
            {
                return NotFound("Задача для удаления не найдена");
            }

            try
            {
                _context.ToDoItems.Remove(todoItem);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при удалении задач.");
            }
        }

        [HttpDelete("delete-all")]
        public IActionResult DeleteAll()
        {
            try
            {
                _databaseCleaner.ClearTodoItems();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при удалении задач.");
            }
        }
    }
}
