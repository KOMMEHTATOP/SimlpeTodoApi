using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleToDoApi.Controllers;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using Xunit;

namespace SimpleToDoApiTest
{
    public class TodoItemsControllerTests
    {
        [Fact]
        public void GetTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<TodoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns((TodoItem)null); // Явно указываем ID = 1

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object, null);

            // Act
            var result = controller.GetTodoItem(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);

            // Verify: Проверяем, что Find вызывался с ID = 1
            mockDbSet.Verify(m => m.Find(1), Times.Once);
        }

        [Fact]
        public void GetTodoItem_ReturnsTodoItem_WhenTaskExists()
        {
            // Arrange
            var testTodoItem = new TodoItem { Id = 1, Title = "Test Task", IsComplete = false };

            var mockDbSet = new Mock<DbSet<TodoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns(testTodoItem);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object, null);

            // Act
            var result = controller.GetTodoItem(1);

            // Assert
            var returnedItem = Assert.IsType<TodoItem>(result.Value); // Проверяем result.Value, а не result.Result
            Assert.Equal(testTodoItem.Id, returnedItem.Id);
            Assert.Equal(testTodoItem.Title, returnedItem.Title);
        }
    }
}