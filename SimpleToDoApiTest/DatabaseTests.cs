using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using Xunit;

public class DatabaseTests
{
    private readonly DbContextOptions<TodoContext> _options;
    public DatabaseTests()
    {
        _options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void AddToDoItem_ShouldAddItemToDatabase()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            var todoItem = new ToDoItem
            {
                Title = "Test Task",
                Description = "desc",
                IsComplete = false,
                Updated = DateTime.Now,
                CreatedByUserId = user.Id
            };
            context.ToDoItems.Add(todoItem);
            context.SaveChanges();

            var addedItem = context.ToDoItems
                .Include(t => t.CreatedByUser)
                .FirstOrDefault(t => t.Title == "Test Task");
            Assert.NotNull(addedItem);
            Assert.Equal("Test Task", addedItem.Title);
            Assert.False(addedItem.IsComplete);
            Assert.Equal(user.Id, addedItem.CreatedByUserId);
        }
    }

    [Fact]
    public void RemoveToDoItem_FromDatabase()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            var todoItem1 = new ToDoItem { Title = "Task1", Description = "", IsComplete = true, Updated = DateTime.Now, CreatedByUserId = user.Id };
            var todoItem2 = new ToDoItem { Title = "Task2", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id };
            context.ToDoItems.AddRange(todoItem1, todoItem2);
            context.SaveChanges();

            context.ToDoItems.Remove(todoItem2);
            context.SaveChanges();
            Assert.Single(context.ToDoItems);
        }
    }

    [Fact]
    public void AccessItem_ByIndex()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            context.ToDoItems.Add(new ToDoItem { Title = "Task1", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.ToDoItems.Add(new ToDoItem { Title = "Task2", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.SaveChanges();

            var items = context.ToDoItems.OrderBy(t => t.Id).ToList();
            Assert.Equal("Task2", items[1].Title);
        }
    }

    [Fact]
    public void UpdateTodoItem_InDatabase()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            var todoItem = new ToDoItem { Title = "Test Task", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id };
            context.ToDoItems.Add(todoItem);
            context.SaveChanges();

            var itemToUpdate = context.ToDoItems.FirstOrDefault(t => t.Title == "Test Task");
            itemToUpdate.IsComplete = true;
            itemToUpdate.Updated = DateTime.Now;
            context.SaveChanges();

            var updatedItem = context.ToDoItems.FirstOrDefault(t => t.Title == "Test Task");
            Assert.NotNull(updatedItem);
            Assert.True(updatedItem.IsComplete);
        }
    }

    [Fact]
    public void GetAllTodoItems_FromDatabase()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            context.ToDoItems.Add(new ToDoItem { Title = "Task 1", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.ToDoItems.Add(new ToDoItem { Title = "Task 2", Description = "", IsComplete = true, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.SaveChanges();

            var allItems = context.ToDoItems.ToList();
            Assert.Equal(2, allItems.Count);
            Assert.Contains(allItems, t => t.Title == "Task 1");
            Assert.Contains(allItems, t => t.Title == "Task 2");
        }
    }

    [Fact]
    public void GetCompletedTodoItems_FromDatabase()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            context.ToDoItems.Add(new ToDoItem { Title = "Task 1", Description = "", IsComplete = false, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.ToDoItems.Add(new ToDoItem { Title = "Task 2", Description = "", IsComplete = true, Updated = DateTime.Now, CreatedByUserId = user.Id });
            context.SaveChanges();

            var completedItems = context.ToDoItems.Where(t => t.IsComplete).ToList();
            Assert.Single(completedItems);
            Assert.Equal("Task 2", completedItems[0].Title);
        }
    }

    [Fact]
    public void GetTodoItem_ThatDoesNotExist()
    {
        using (var context = new TodoContext(_options))
        {
            var user = new User { UserName = "testuser", Email = "test@mail.com", PasswordHash = "hash" };
            context.Users.Add(user);
            context.SaveChanges();

            var item = context.ToDoItems.FirstOrDefault(t => t.Title == "Nonexistent Task");
            Assert.Null(item);
        }
    }
}