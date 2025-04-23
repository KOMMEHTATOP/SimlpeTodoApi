using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleToDoApi.Controllers;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using MockQueryable.Moq;
using SimpleToDoApi.DTO.ToDoItem;
using System.Linq.Expressions;

namespace SimpleToDoApiTest
{
    public class TodoItemsControllerTests
    {
        //-------------------------GetTodoItems (GET /view-all-tasks)--------------------------
        [Fact]
        public void GetTodoItems_ReturnsAllItems_WhenDatabaseHasItems()
        {
            var listTasks = new List<ToDoItem>
            {
                new ToDoItem {Id=1, Title="Title1", IsComplete=false},
                new ToDoItem {Id=2, Title="Title2", IsComplete=true},
                new ToDoItem {Id=3, Title="Title3", IsComplete=false}
            }.AsQueryable();

            var mockDbSet = listTasks.AsQueryable().BuildMockDbSet(); 
            //МЕТОД ВЫШЕ ЗАМЕНЯЕТ ВОТ ТАКУЮ СБОРКУ
            //var mockDbSet = new Mock<DbSet<ToDoItem>>();
            //mockDbSet.As<IQueryable<ToDoItem>>().Setup(m => m.Provider).Returns(listTasks.Provider);
            //mockDbSet.As<IQueryable<ToDoItem>>().Setup(m => m.Expression).Returns(listTasks.Expression);
            //mockDbSet.As<IQueryable<ToDoItem>>().Setup(m => m.ElementType).Returns(listTasks.ElementType);
            //mockDbSet.As<IQueryable<ToDoItem>>().Setup(m => m.GetEnumerator()).Returns(listTasks.GetEnumerator());

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            var result = controller.GetTodoItems();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<ToDoItem>>(okResult.Value);
            Assert.Equal(3, returnedItems.Count());
        }

        //-------------------------GetTodoItem (GET /view-task-ID-{id})--------------------------
        [Fact]
        public void GetTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns((ToDoItem?)null); // Явно указываем ID = 1

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

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
            var testTodoItem = new ToDoItem { Id = 1, Title = "Test Task", IsComplete = false };

            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns(testTodoItem);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.GetTodoItem(1);

            // Assert
            var returnedItem = Assert.IsType<ToDoItem>(result.Value); 
            Assert.Equal(testTodoItem.Id, returnedItem.Id);
            Assert.Equal(testTodoItem.Title, returnedItem.Title);
        }


        //-------------------------PostTodoItem (POST /create-new-task)--------------------------
        [Fact]
        public void PostTodoItem_ReturnsCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var validToDoItemDto = new CreateToDoItemDto { Title = "TestTask", Description = "Test Description", IsComplete = false };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.ToDoItems.Any(It.IsAny<Expression<Func<ToDoItem, bool>>>())).Returns(false);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.PostTodoItem(validToDoItemDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtResult.StatusCode);

            var returnedTask = Assert.IsType<ToDoItemDto>(createdAtResult.Value);
            Assert.Equal(validToDoItemDto.Title, returnedTask.Title);
            Assert.Equal(validToDoItemDto.Description, returnedTask.Description);
            Assert.Equal(validToDoItemDto.IsComplete, returnedTask.IsComplete);

            mockDbSet.Verify(m => m.Add(It.Is<ToDoItem>(t => 
                t.Title == validToDoItemDto.Title && 
                t.Description == validToDoItemDto.Description && 
                t.IsComplete == validToDoItemDto.IsComplete && 
                t.CreatedByUserId == null && 
                t.Updated != default)), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
        
        [Fact]
        public void PostTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);

            // Имитируем невалидный DTO
            var invalidItem = new CreateToDoItemDto { Title = "", IsComplete = false }; // Пустой Title вызовет ошибку валидации
            controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = controller.PostTodoItem(invalidItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            // Проверяем, что вернулись ошибки валидации
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }
        //-------------------------UpdateTodoItem (PUT /update-task-{id})--------------------------

        [Fact]
        public void UpdateTodoItem_UpdatesItem_WhenModelIsValid()
        {
            // Arrange
            var existingItem = new ToDoItem { Id = 1, Title = "OldTitle", Description = "OldDesc", IsComplete = false };
            var updatedItemDto = new UpdateToDoItemDto { Title = "NewTitle", Description = "NewDesc", IsComplete = true };

            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);

            mockContext.Setup(c => c.ToDoItems.Find(1)).Returns(existingItem);
            mockContext.Setup(c => c.ToDoItems.Any(It.IsAny<Expression<Func<ToDoItem, bool>>>())).Returns(false); // Для проверки уникальности
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = controller.UpdateTodoItem(1, updatedItemDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedTask = Assert.IsType<ToDoItemDto>(okResult.Value);
            Assert.Equal(updatedItemDto.Title, returnedTask.Title);
            Assert.Equal(updatedItemDto.Description, returnedTask.Description);
            Assert.Equal(updatedItemDto.IsComplete, returnedTask.IsComplete);

            Assert.Equal(updatedItemDto.Title, existingItem.Title);
            Assert.Equal(updatedItemDto.Description, existingItem.Description);
            Assert.Equal(updatedItemDto.IsComplete, existingItem.IsComplete);

            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
        
        [Fact]
        public void UpdateTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var invalidItemDto = new UpdateToDoItemDto { Title = "", Description = "Desc", IsComplete = false };
            var mockContext = new Mock<ITodoContext>();

            var controller = new TodoItemsController(mockContext.Object);
            controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = controller.UpdateTodoItem(1, invalidItemDto);

            // Assert
            var errorResponse = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, errorResponse.StatusCode);

            // Проверка содержимого ошибки
            Assert.NotNull(errorResponse.Value);
            var serializableError = Assert.IsType<SerializableError>(errorResponse.Value);
            Assert.True(serializableError.ContainsKey("Title"));
        }
        
        [Fact]
        public void UpdateTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var newTodoItemDto = new UpdateToDoItemDto { Title = "New Title", Description = "Desc", IsComplete = false };
            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);
            mockContext.Setup(c => c.ToDoItems.Find(It.IsAny<int>())).Returns((ToDoItem)null!);

            // Act
            var result = controller.UpdateTodoItem(1, newTodoItemDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Задача не найдена!", notFoundResult.Value);
        }
        
        //-------------------------DeleteTodoItem (DELETE /delete-task-{id})--------------------------
        [Fact]
        public void DeleteTodoItem_DeletesItem_WhenTaskExists()
        {
            var itemToDelete = new ToDoItem
            {
                Id = 1, Title = "OldTitle"
            };
            var mockContext = new Mock<ITodoContext>();
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            
            mockDbSet.Setup(c=>c.Find(1)).Returns(itemToDelete);
            mockDbSet.Setup(c=>c.Remove(itemToDelete)).Verifiable();
            mockContext.Setup(c=>c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new TodoItemsController(mockContext.Object);
            var result = controller.DeleteTodoItem(itemToDelete.Id);
            Assert.IsType<NoContentResult>(result);
            mockDbSet.Verify(m => m.Remove(itemToDelete), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var mockContext = new Mock<ITodoContext>();
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(c=>c.Find(1)).Returns((ToDoItem)null!);
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var controller = new TodoItemsController(mockContext.Object);
            var result = controller.DeleteTodoItem(1);
            Assert.IsType<NotFoundResult>(result);
            mockDbSet.Verify(m => m.Remove(It.IsAny<ToDoItem>()), Times.Never);
            mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }
    }
}