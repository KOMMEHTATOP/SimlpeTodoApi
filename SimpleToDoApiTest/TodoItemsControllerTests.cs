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
            // Arrange
            var listTasks = new List<ToDoItem>
            {
                new ToDoItem
                {
                    Id = 1, Title = "Title1", Description = "Desc1", IsComplete = false
                },
                new ToDoItem
                {
                    Id = 2, Title = "Title2", Description = "Desc2", IsComplete = true
                },
                new ToDoItem
                {
                    Id = 3, Title = "Title3", Description = "Desc3", IsComplete = false
                }
            }.AsQueryable();

            var mockDbSet = listTasks.BuildMockDbSet();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.GetTodoItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<ToDoItemDto>>(okResult.Value);

            Assert.Equal(3, returnedItems.Count());
            Assert.Contains(returnedItems, item => item.Title == "Title1");
            Assert.Contains(returnedItems, item => item.Title == "Title2");
            Assert.Contains(returnedItems, item => item.Title == "Title3");
        }

        //-------------------------GetTodoItem (GET /view-task-ID-{id})--------------------------
        [Fact]
        public void GetTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns((ToDoItem?)null);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.GetTodoItem(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            mockDbSet.Verify(m => m.Find(1), Times.Once);
        }

        [Fact]
        public void GetTodoItem_ReturnsTodoItem_WhenTaskExists()
        {
            // Arrange
            var testTodoItem = new ToDoItem
            {
                Id = 1, Title = "Test Task", Description = "Test Desc", IsComplete = false
            };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns(testTodoItem);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.GetTodoItem(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // result.Result, не result.Value!
            var returnedItem = Assert.IsType<ToDoItemDto>(okResult.Value);
            Assert.Equal(testTodoItem.Title, returnedItem.Title);
            Assert.Equal(testTodoItem.Description, returnedItem.Description);
            Assert.Equal(testTodoItem.IsComplete, returnedItem.IsComplete);
        }


        //-------------------------PostTodoItem (POST /create-new-task)--------------------------
        [Fact]
        public void PostTodoItem_ReturnsCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var validToDoItemDto = new CreateToDoItemDto
            {
                Title = "TestTask", Description = "Test Description", IsComplete = false
            };

            // Нет задач с таким названием => Any вернёт false
            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
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
                t.IsComplete == validToDoItemDto.IsComplete)), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }


        [Fact]
        public void PostTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);

            var invalidItem = new CreateToDoItemDto
            {
                Title = "", IsComplete = false
            };
            controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = controller.PostTodoItem(invalidItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }

        //-------------------------UpdateTodoItem (PUT /update-task-{id})--------------------------
        [Fact]
        public void UpdateTodoItem_UpdatesItem_WhenModelIsValid()
        {
            // Arrange
            var existingItem = new ToDoItem
            {
                Id = 1, Title = "OldTitle", Description = "OldDesc", IsComplete = false
            };
            var updatedItemDto = new UpdateToDoItemDto
            {
                Title = "NewTitle", Description = "NewDesc", IsComplete = true
            };

            var listTasks = new List<ToDoItem>
            {
                existingItem
            }.AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.Find(1)).Returns(existingItem);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.UpdateTodoItem(1, updatedItemDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // исправлено!
            var returnedTask = Assert.IsType<ToDoItemDto>(okResult.Value);
            Assert.Equal(updatedItemDto.Title, returnedTask.Title);
            Assert.Equal(updatedItemDto.Description, returnedTask.Description);
            Assert.Equal(updatedItemDto.IsComplete, returnedTask.IsComplete);

            // Дополнительно: убедиться, что изменения были применены к сущности
            Assert.Equal(updatedItemDto.Title, existingItem.Title);
            Assert.Equal(updatedItemDto.Description, existingItem.Description);
            Assert.Equal(updatedItemDto.IsComplete, existingItem.IsComplete);

            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        //-------------------------DeleteTodoItem (DELETE /delete-task-{id})--------------------------
        [Fact]
        public void DeleteTodoItem_DeletesItem_WhenTaskExists()
        {
            // Arrange
            var itemToDelete = new ToDoItem
            {
                Id = 1, Title = "OldTitle"
            };

            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.Find(1)).Returns(itemToDelete);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.DeleteTodoItem(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockDbSet.Verify(m => m.Remove(itemToDelete), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }


        // Проверка NotFound при попытке обновить несуществующую задачу
        [Fact]
        public void UpdateTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var updatedItemDto = new UpdateToDoItemDto
            {
                Title = "NewTitle", Description = "NewDesc", IsComplete = true
            };
            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.Find(1)).Returns((ToDoItem)null);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.UpdateTodoItem(1, updatedItemDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Задача не найдена!", notFoundResult.Value);
        }

// Проверка BadRequest при попытке создать задачу с уже существующим названием
        [Fact]
        public void PostTodoItem_ReturnsBadRequest_WhenTitleAlreadyExists()
        {
            // Arrange
            var existingTask = new ToDoItem
            {
                Id = 1, Title = "TestTask", Description = "Desc", IsComplete = false
            };
            var listTasks = new List<ToDoItem>
            {
                existingTask
            }.AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            var newItem = new CreateToDoItemDto
            {
                Title = "TestTask", Description = "Desc2", IsComplete = true
            };

            // Act
            var result = controller.PostTodoItem(newItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains("существует", badRequestResult.Value.ToString());
        }

// Проверка BadRequest при попытке обновить задачу с дублирующим заголовком
        [Fact]
        public void UpdateTodoItem_ReturnsBadRequest_WhenTitleAlreadyExists()
        {
            // Arrange
            var existingTask = new ToDoItem
            {
                Id = 1, Title = "OldTitle", Description = "Desc1", IsComplete = false
            };
            var otherTask = new ToDoItem
            {
                Id = 2, Title = "NewTitle", Description = "Desc2", IsComplete = true
            };
            var listTasks = new List<ToDoItem>
            {
                existingTask, otherTask
            }.AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.Find(1)).Returns(existingTask);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            var updateDto = new UpdateToDoItemDto
            {
                Title = "NewTitle", Description = "Any", IsComplete = true
            };

            // Act
            var result = controller.UpdateTodoItem(1, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains("существует", badRequestResult.Value.ToString());
        }

// Проверка NotFound при попытке удалить несуществующую задачу
        [Fact]
        public void DeleteTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.Find(5)).Returns((ToDoItem)null);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);

            var controller = new TodoItemsController(mockContext.Object);

            // Act
            var result = controller.DeleteTodoItem(5);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("не найдена", notFoundResult.Value.ToString());
        }

// Проверка BadRequest при невалидном DTO при обновлении задачи
        [Fact]
        public void UpdateTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);

            var invalidDto = new UpdateToDoItemDto { Title = "", Description = "Desc", IsComplete = false };
            controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = controller.UpdateTodoItem(1, invalidDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result); // Исправлено здесь!
            Assert.Equal(400, badRequestResult.StatusCode);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }    
        
    }
}
