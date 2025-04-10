using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }

        public DbSet<TodoItem> ToDoItems { get; set; }
    }
}
