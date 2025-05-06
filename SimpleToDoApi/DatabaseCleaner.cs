using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;

namespace SimpleToDoApi.Services;

public class DatabaseCleaner : IDatabaseCleaner
{
    private readonly TodoContext _context;
    public DatabaseCleaner(TodoContext context)
    {
        _context = context;
    }

    public async Task ClearTodoItems()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [TodoItems]; DBCC CHECKIDENT ('[TodoItems]', RESEED, 0);");
    }

    public async Task ClearUsers()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Users]; DBCC CHECKIDENT ('[Users]', RESEED, 0);");
    }

    public async Task ClearRoles()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Roles]; DBCC CHECKIDENT ('[Roles]', RESEED, 0);");
    }
}
