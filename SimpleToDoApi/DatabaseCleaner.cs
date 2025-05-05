using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;

namespace SimpleToDoApi
{
    public class DatabaseCleaner(TodoContext context) : IDatabaseCleaner
    {
        // Удалить все задачи
        public async Task ClearTodoItems()
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [TodoItems]; DBCC CHECKIDENT ('[TodoItems]', RESEED, 0);");
        }

        // Удалить всех пользователей
        public async Task ClearUsers()
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Users]; DBCC CHECKIDENT ('[Users]', RESEED, 0);");
        }
        
        // Удалить все роли
        public async Task ClearRoles()
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Roles]; DBCC CHECKIDENT ('[Roles]', RESEED, 0);");
        }
    }
}