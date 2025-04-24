using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;

namespace SimpleToDoApi
{
    public class DatabaseCleaner
    {
        private readonly TodoContext _context;
        private DatabaseCleaner(TodoContext context)
        {
            _context = context;
        }

        // Удалить все задачи
        public async Task ClearTodoItems()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [TodoItems]; DBCC CHECKPOINT ('[TodoItems]', RESEED, 0);");
        }

        // Удалить всех пользователей
        public async Task ClearUsers()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Users]; DBCC CHECKIDENT ('[Users]', RESEED, 0);");
        }
        
        // Удалить все роли
        public async Task ClearRoles()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Roles]; DBCC CHECKIDENT ('[Roles]', RESEED, 0);");
        }
    }
}