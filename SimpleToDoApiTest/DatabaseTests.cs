using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using System.Collections.Generic;

namespace SimpleToDoApiTest
{
    public class DatabaseTests
    {
        private readonly DbContextOptions<TodoContext> _options;
        public DatabaseTests()
        {
            // Общая настройка базы данных для всех тестов
            _options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникальная база данных для каждого теста
                .Options;
        }

        [Fact]
        public void AddToDoItem_ShouldAddItemToDatabase()
        {
            using (var context = new TodoContext(_options))
            {
                // Act: Добавляем элемент в базу данных
                var todoItem = new TodoItem { Title = "Test Task", IsComplete = false };
                context.ToDoItems.Add(todoItem);
                context.SaveChanges();

                // Assert: Проверяем, что элемент добавлен
                var addedItem = context.ToDoItems.FirstOrDefault(t => t.Title == "Test Task");
                Assert.NotNull(addedItem); // Элемент существует
                Assert.Equal("Test Task", addedItem.Title); // Имя совпадает
                Assert.False(addedItem.IsComplete); // Статус совпадает
            }
        }

        [Fact]
        public void RemoveToDoItem_FromDatabase()
        {
            using (var context = new TodoContext(_options))
            {
                var todoItem = new TodoItem { Title = "Test Task", IsComplete = false };
                var todoItem1 = new TodoItem { Title = "Test Task1", IsComplete = true };
                var todoItem2 = new TodoItem { Title = "Test Task2", IsComplete = false };
                context.ToDoItems.Add(todoItem);
                context.ToDoItems.Add(todoItem1);
                context.ToDoItems.Add(todoItem2);
                context.SaveChanges();

                var checkRemoveTodoItem = context.ToDoItems.Remove(todoItem2);
                context.SaveChanges();
                Assert.Equal(2, context.ToDoItems.Count());
            }
        }

        [Fact]
        public void AccessItem_ByIndex()
        {
            using (var context = new TodoContext(_options))
            {
                // Arrange: Добавляем элементы
                var todoItem = new TodoItem { Title = "Test Task", IsComplete = false };
                var todoItem1 = new TodoItem { Title = "Test Task1", IsComplete = false };
                var todoItem2 = new TodoItem { Title = "Test Task1", IsComplete = false };
                context.ToDoItems.Add(todoItem);
                context.ToDoItems.Add(todoItem1);
                context.ToDoItems.Add(todoItem2);
                context.SaveChanges();

                // Act: Преобразуем DbSet в список для доступа по индексу
                var items = context.ToDoItems.ToList();

                // Assert: Проверяем элемент по индексу
                Assert.Equal("Test Task1", items[1].Title);
            }
        }

        [Fact]
        public void UpdateTodoItem_InDatabase()
        {
            using (var context = new TodoContext(_options))
            {
                // Arrange: Добавляем элемент
                var todoItem = new TodoItem { Title = "Test Task", IsComplete = false };
                context.ToDoItems.Add(todoItem);
                context.SaveChanges();

                // Act: Обновляем элемент
                var itemToUpdate = context.ToDoItems.FirstOrDefault(t => t.Title == "Test Task");
                itemToUpdate.IsComplete = true;
                context.SaveChanges();

                // Assert: Проверяем, что элемент обновлён
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
                // Arrange: Добавляем несколько элементов
                context.ToDoItems.Add(new TodoItem { Title = "Task 1", IsComplete = false });
                context.ToDoItems.Add(new TodoItem { Title = "Task 2", IsComplete = true });
                context.SaveChanges();

                // Act: Получаем все элементы
                var allItems = context.ToDoItems.ToList();

                // Assert: Проверяем количество элементов
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
                // Arrange: Добавляем несколько элементов
                context.ToDoItems.Add(new TodoItem { Title = "Task 1", IsComplete = false });
                context.ToDoItems.Add(new TodoItem { Title = "Task 2", IsComplete = true });
                context.SaveChanges();

                // Act: Получаем только завершённые задачи
                var completedItems = context.ToDoItems.Where(t => t.IsComplete).ToList();

                // Assert: Проверяем количество завершённых задач
                Assert.Single(completedItems);
                Assert.Equal("Task 2", completedItems[0].Title);
            }
        }

        [Fact]
        public void GetTodoItem_ThatDoesNotExist()
        {
            using (var context = new TodoContext(_options))
            {
                // Act: Пытаемся получить элемент, которого нет в базе
                var item = context.ToDoItems.FirstOrDefault(t => t.Title == "Nonexistent Task");

                // Assert: Проверяем, что результат null
                Assert.Null(item);
            }
        }
    }
}