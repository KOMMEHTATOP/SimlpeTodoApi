using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly DatabaseCleaner _databaseCleaner;
        public TodoItemsController(TodoContext context, DatabaseCleaner databaseCleaner)
        {
            _context = context;
            _databaseCleaner = databaseCleaner;
        }

        // GET: api/TodoItems
        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetTodoItems()
        {
            return Ok(_context.ToDoItems);
        }

        // GET: api/TodoItems/1
        [HttpGet("{id}")]
        public ActionResult<TodoItem> GetTodoItem(int id)
        {
            var todoItem = _context.ToDoItems.Find(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }


        // POST: api/TodoItems
        [HttpPost]
        public ActionResult<TodoItem> PostTodoItem([FromBody] TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ToDoItems.Add(todoItem);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }


        // PUT: api/TodoItems/1
        [HttpPut("{id}")]
        public IActionResult UpdateTodoItem(int id, TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != todoItem.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            var existingItem = _context.ToDoItems.Find(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Title = todoItem.Title;
            existingItem.IsComplete = todoItem.IsComplete;
            
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "База не доступна " + ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/TodoItems/1
        [HttpDelete("{id}")]
        public IActionResult DeleteTodoItem(int id)
        {
            var todoItem = _context.ToDoItems.Find(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.ToDoItems.Remove(todoItem);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("delete-all")]
        public IActionResult DeleteAll()
        {
            _databaseCleaner.ClearTodoItems();
            return Ok("Все задачи были удалены.");
        }
    }
}
