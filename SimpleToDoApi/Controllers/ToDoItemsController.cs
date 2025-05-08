using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApi.Controllers
{
    [Route("api/todo-items")]
    [ApiController]
    public class ToDoItemsController(IToDoService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ToDoMetaPage<ToDoItemDto>>> GetAllTodoItems([FromQuery] ToDoItemFilterDto filter)
        {
            var result = await service.GetAllToDo(filter);

            if (result.Error != null)
            {
                return BadRequest("User does not exist.");
            }
            return Ok(result.Page);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodoItemById(int id)
        {
            var todoItem = await service.GetByIdToDo(id);

            if (todoItem.Error!=null)
            {
                return NotFound("Task not found.");
            }

            return Ok(todoItem.Item);
        }
     
        [HttpPost]
        public async Task<ActionResult<ToDoItemDto>> CreateTodoItem([FromBody] CreateToDoItemDto createTodoItemDto)
        {
            var result = await service.CreateToDo(createTodoItemDto);

            if (result.Error != null)
            {
                return BadRequest("Title already exists");
            }

            if (result.CreatedItem == null)
            {
                return StatusCode(500, "Unexpected null result");
            }
            
            return CreatedAtAction(nameof(GetTodoItemById), new {id = result.CreatedItem.Id }, result.CreatedItem);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ToDoItemDto>> UpdateTodoItem(
            [FromRoute] int id, [FromBody] UpdateToDoItemDto updateDto)
        {
            var result = await service.UpdateToDo(id, updateDto);

            if (result.Error != null)
            {
                switch (result.Error)
                {
                    case UpdateToDoItemResult.UpdateTodoItemResult.ItemNotFound:
                        return NotFound("Task not found.");
                    case UpdateToDoItemResult.UpdateTodoItemResult.TitleExists:
                        return BadRequest("Title already exists");
                }
            }
            
            return Ok(result.UpdatedToDoItem);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoItem(int id)
        {
            var deleted = await service.DeleteId(id);

            if (!deleted)
            {
                return NotFound("Task not found.");
            }
            
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAllTodoItems()
        {
            var deleted = await service.DeleteAll();

            if (!deleted)
            {
                return NotFound("No tasks to delete.");
            }
            
            return NoContent();
        }
    }
}
