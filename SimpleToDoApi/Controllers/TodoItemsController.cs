using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;

namespace SimpleToDoApi.Controllers
{
    [Route("api/todo-items")]
    [ApiController]
    public class TodoItemsController(ITodoContext context, IDatabaseCleaner databaseCleaner) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<ToDoItemDto>> CreateTodoItem([FromBody] CreateToDoItemDto createTodoItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await context.ToDoItems.AnyAsync(t => t.Title == createTodoItemDto.Title))
            {
                return BadRequest("A task with this title already exists.");
            }

            var newToDoItem = ToDoItemMapper.FromDto(createTodoItemDto);

            context.ToDoItems.Add(newToDoItem);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItemById), new { id = newToDoItem.Id }, ToDoItemMapper.ToDto(newToDoItem));
        }

        [HttpGet]
        public async Task<ActionResult<List<ToDoItemDto>>> GetAllTodoItems()
        {
            var items = await context.ToDoItems.ToListAsync();
            var tasks = items.Select(ToDoItemMapper.ToDto).ToList();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodoItemById(int id)
        {
            var todoItem = await context.ToDoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(ToDoItemMapper.ToDto(todoItem));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ToDoItemDto>> UpdateTodoItem([FromRoute] int id, [FromBody] UpdateToDoItemDto newToDoItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await context.ToDoItems.AnyAsync(t => t.Title == newToDoItemDto.Title && t.Id != id))
            {
                return BadRequest("A task with this title already exists.");
            }

            var existingTodoItem = await context.ToDoItems.FindAsync(id);

            if (existingTodoItem == null)
            {
                return NotFound("Task not found.");
            }

            existingTodoItem.Title = newToDoItemDto.Title;
            existingTodoItem.Description = newToDoItemDto.Description;
            existingTodoItem.IsComplete = newToDoItemDto.IsComplete;
            existingTodoItem.Updated = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Ok(ToDoItemMapper.ToDto(existingTodoItem));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await context.ToDoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound("Task not found.");
            }

            context.ToDoItems.Remove(todoItem);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAllTodoItems()
        {
            await databaseCleaner.ClearTodoItems();
            return NoContent();
        }
    }
}