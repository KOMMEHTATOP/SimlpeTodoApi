using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController(ITodoContext context, IDatabaseCleaner databaseCleaner) : ControllerBase
    {
        [HttpPost("create-new-task")]
        public async Task<ActionResult<ToDoItemDto>> PostTodoItem([FromBody] CreateToDoItemDto createTodoItemDto)
        {
            if (await context.ToDoItems.AnyAsync(t => t.Title == createTodoItemDto.Title))
            {
                return BadRequest(
                    "Задача с таким заголовком уже существует. Скоректируйте/удалите существующую или создайте задачу с новым заголовком.");
            }

            var newToDoItem = ToDoItemMapper.FromDto(createTodoItemDto);

            context.ToDoItems.Add(newToDoItem);

            try
            {
                await context.SaveChangesAsync();
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
        public async Task<ActionResult<List<ToDoItemDto>>> GetTodoItems()
        {
            var items = await context.ToDoItems.ToListAsync();
            var tasks = items.Select(ToDoItemMapper.ToDto).ToList();
            return Ok(tasks);
        }

        // GET: api/TodoItems/1
        [HttpGet("view-task-ID-{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodoItem(int id)
        {
            var todoItem = await context.ToDoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound("Задача в базе данных не найдена");
            }

            return Ok(ToDoItemMapper.ToDto(todoItem));
        }

        // PUT: api/TodoItems/1
        [HttpPut("update-task-{id}")]
        public async Task<ActionResult<ToDoItemDto>> UpdateTodoItem([FromRoute] int id,
            [FromBody] UpdateToDoItemDto newToDoItemDto)
        {
            if (context.ToDoItems.Any(t => t.Title == newToDoItemDto.Title && t.Id != id))
            {
                return BadRequest("Задача с таким заголовком уже существует.");
            }

            var existingTodoItem = await context.ToDoItems.FindAsync(id);

            if (existingTodoItem == null)
            {
                return NotFound("Задача не найдена!");
            }

            existingTodoItem.Title = newToDoItemDto.Title;
            existingTodoItem.Description = newToDoItemDto.Description;
            existingTodoItem.IsComplete = newToDoItemDto.IsComplete;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "База не доступна " + ex.Message);
            }

            return Ok(ToDoItemMapper.ToDto(existingTodoItem));
        }

        // DELETE: api/TodoItems/1
        [HttpDelete("delete-task-{id}")]
        public async Task<ActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await context.ToDoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound("Задача для удаления не найдена");
            }

            context.ToDoItems.Remove(todoItem);

            try
            {
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, "Ошибка при удалении задач. " + e.Message);
            }
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                await databaseCleaner.ClearTodoItems();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при удалении задач.");
            }
        }
    }
}
