using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public class TodoContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, ITodoContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }

        // Fluent Api - по сути надстройка для БД. Тут можно установить обязательные поля, длинну и т.д.
        // Пока не понимаю зачем это, так как это можно сделать из кода .NET
        // protected override void OnModelCreating(ModelBuilder builder)
        // {
        //     base.OnModelCreating(builder);
        // }
        
        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
}
