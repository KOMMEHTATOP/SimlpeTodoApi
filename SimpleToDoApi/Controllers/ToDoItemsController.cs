using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApi.Controllers
{
    [Route("api/todo-items")]
    [ApiController]
    public class ToDoItemsController: ControllerBase
    {
        private readonly IToDoService _service;
        public ToDoItemsController(IToDoService service)
        {
            _service = service;
        }
        
        [HttpPost]
        public async Task<ActionResult<ToDoItemDto>> CreateTodoItem([FromBody] CreateToDoItemDto createTodoItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _service.CreateAsync(createTodoItemDto);

            if (created == null)
            {
                return BadRequest("A task with this title already exists.");
            }

            return CreatedAtAction(nameof(GetTodoItemById), new { id = created.Id }, created);        
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedResult<ToDoItemDto>>> GetAllTodoItems([FromQuery] ToDoItemFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodoItemById(int id)
        {
            var todoItem = await _service.GetByIdAsync(id);

            if (todoItem == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(todoItem);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<ToDoItemDto>> UpdateTodoItem(
            [FromRoute] int id, [FromBody] UpdateToDoItemDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, updateDto);

            if (updated == null)
                return NotFound("Item not found or title already exists.");

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoItem(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound("Task not found.");
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAllTodoItems()
        {
            var deleted = await _service.DeleteAllAsync();
            if (!deleted)
                return NotFound("No tasks to delete.");
            return NoContent();
        }
    }
}
