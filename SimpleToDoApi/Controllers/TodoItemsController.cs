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

            return CreatedAtAction(nameof(GetTodoItemById), new
            {
                id = newToDoItem.Id
            }, ToDoItemMapper.ToDto(newToDoItem));
        }

        [HttpGet]
        public async Task<ActionResult<List<ToDoItemDto>>> GetAllTodoItems(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isComplete = null,
            [FromQuery] string search = null,
            [FromQuery] int userId = 0
        )
        {
            var query = context.ToDoItems.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            if (isComplete.HasValue)
            {
                query = query.Where(t => t.IsComplete == isComplete.Value);
            }

            if (userId > 0)
            {
                var userExists = await context.Users.AnyAsync(u => u.Id == userId);

                if (!userExists)
                {
                    return BadRequest("User does not exist.");
                }
                query = query.Where(i=>i.CreatedByUserId == userId);
            }
            
            

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(ToDoItemMapper.ToDto).ToList();

            return Ok(new
            {
                totalCount, page, pageSize, data = dtos
            });
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
        public async Task<ActionResult<ToDoItemDto>> UpdateTodoItem([FromRoute] int id,
            [FromBody] UpdateToDoItemDto newToDoItemDto)
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
