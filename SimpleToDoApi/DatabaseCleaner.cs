using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;

namespace SimpleToDoApi
{
    public class DatabaseCleaner
    {
        private readonly TodoContext _context;

        public DatabaseCleaner(TodoContext context)
        {
            _context = context;
        }

        // Удалить все задачи
        public void ClearTodoItems()
        {
            _context.Database.ExecuteSqlRaw("DELETE FROM [TodoItems]; DBCC CHECKIDENT ('[TodoItems]', RESEED, 0);");
            _context.SaveChanges();
        }

        // Удалить всех пользователей
        public void ClearUsers()
        {
            _context.Database.ExecuteSqlRaw("DELETE FROM [Users]; DBCC CHECKIDENT ('[Users]', RESEED, 0);");
            _context.SaveChanges();
        }
    }
}