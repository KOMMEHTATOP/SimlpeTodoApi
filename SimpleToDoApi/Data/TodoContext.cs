using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }

        public DbSet<TodoItem> ToDoItems { get; set; }
        public DbSet<UserDTO> Users { get; set; }
    }
}
