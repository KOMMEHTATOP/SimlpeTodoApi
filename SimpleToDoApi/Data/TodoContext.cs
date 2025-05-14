using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public class TodoContext(DbContextOptions<TodoContext> options) : IdentityDbContext<User, Role, string>(options), ITodoContext
    {
        public DbSet<ToDoItem> ToDoItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
