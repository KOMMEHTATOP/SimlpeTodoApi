using Microsoft.EntityFrameworkCore;
using SimlpeTodoApi.Models;

namespace SimlpeTodoApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }

        public DbSet<TodoItem> ToDoItems { get; set; }
    }
}
