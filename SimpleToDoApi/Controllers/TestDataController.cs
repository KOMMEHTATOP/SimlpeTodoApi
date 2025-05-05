using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers;

[ApiController]
[Route("api/test-data")]
public class TestDataController(ITodoContext context) : ControllerBase
{
    [HttpPost("generate-todo-items")]
    public async Task<IActionResult> GenerateTodoItems([FromQuery] int count = 100)
    {
        Random rnd = new Random();
        for (int i = 0; i < count; i++)
        {
            int userId = rnd.Next(1, 4);
            context.ToDoItems.Add(new ToDoItem
            {
                Title = $"Task {i + 1}",
                Description = $"Description for task {i + 1}",
                IsComplete = i % 2 == 0,
                CreatedByUserId = userId
            });
        }

        await context.SaveChangesAsync();
        return Ok($"{count} todo items generated.");
    }
}
