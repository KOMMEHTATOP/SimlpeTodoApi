using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers;

[ApiController]
[Route("api/test-data")]
public class TestDataController : ControllerBase
{
    private readonly ITodoContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TestDataController(ITodoContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    [HttpPost("generate-todo-items")]
    public async Task<IActionResult> GenerateTodoItems([FromQuery] int count = 100)
    {
        var users = await _userManager.Users.ToListAsync();
        if (!users.Any())
        {
            return BadRequest("No users found. Create users first.");
        }

        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            var randomUser = users[random.Next(users.Count)];
            _context.ToDoItems.Add(new ToDoItem
            {
                Title = $"Task {i + 1}",
                Description = $"Description for task {i + 1}",
                IsComplete = i % 2 == 0,
                CreatedByUserId = randomUser.Id 
            });
        }

        await _context.SaveChangesAsync();
        return Ok($"{count} todo items generated for {users.Count} users.");
    }
}