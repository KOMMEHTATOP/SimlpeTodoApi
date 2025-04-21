using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public class TodoContext : DbContext, ITodoContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
    }
}
