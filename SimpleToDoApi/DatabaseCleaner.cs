using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;

namespace SimpleToDoApi;

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
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Users]; DBCC CHECKIDENT ('[AspNetUsers]', RESEED, 0);");
    }

    public async Task ClearRoles()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Roles]; DBCC CHECKIDENT ('[AspNetRoles]', RESEED, 0);");
    }
}
