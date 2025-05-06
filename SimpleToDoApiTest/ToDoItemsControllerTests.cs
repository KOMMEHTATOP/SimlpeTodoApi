using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleToDoApi.Controllers;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using MockQueryable.Moq;
using Moq.EntityFrameworkCore;
using SimpleToDoApi.DTO.ToDoItem;
using System.Linq.Expressions;

namespace SimpleToDoApiTest
{
    public class ToDoItemsControllerTests
    {
        [Fact]
        public async Task GetTodoItems_ReturnsAllItems_WhenDatabaseHasItems()
        {
            var listTasks = new List<ToDoItem>
            {
                new ToDoItem { Id = 1, Title = "Title1", Description = "Desc1", IsComplete = false },
                new ToDoItem { Id = 2, Title = "Title2", Description = "Desc2", IsComplete = true },
                new ToDoItem { Id = 3, Title = "Title3", Description = "Desc3", IsComplete = false }
            }.AsQueryable();

            var mockDbSet = listTasks.BuildMockDbSet();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var mockCleaner = new Mock<IDatabaseCleaner>();
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.GetAllTodoItems();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<ToDoItemDto>>(okResult.Value);

            Assert.Equal(3, returnedItems.Count());
            Assert.Contains(returnedItems, item => item.Title == "Title1");
            Assert.Contains(returnedItems, item => item.Title == "Title2");
            Assert.Contains(returnedItems, item => item.Title == "Title3");
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((ToDoItem?)null);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var mockCleaner = new Mock<IDatabaseCleaner>();
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.GetTodoItemById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Task not found.", notFoundResult.Value);
            mockDbSet.Verify(m => m.FindAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsTodoItem_WhenTaskExists()
        {
            var testTodoItem = new ToDoItem
            {
                Id = 1, Title = "Test Task", Description = "Test Desc", IsComplete = false
            };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(testTodoItem);

            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var mockCleaner = new Mock<IDatabaseCleaner>();
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.GetTodoItemById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedItem = Assert.IsType<ToDoItemDto>(okResult.Value);
            Assert.Equal(testTodoItem.Title, returnedItem.Title);
            Assert.Equal(testTodoItem.Description, returnedItem.Description);
            Assert.Equal(testTodoItem.IsComplete, returnedItem.IsComplete);
        }

        [Fact]
        public async Task PostTodoItem_ReturnsCreatedAtActionResult_WhenModelIsValid()
        {
            var validToDoItemDto = new CreateToDoItemDto
            {
                Title = "TestTask", Description = "Test Description", IsComplete = false
            };

            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.CreateTodoItem(validToDoItemDto);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);

            var returnedTask = Assert.IsType<ToDoItemDto>(createdAtResult.Value);
            Assert.Equal(validToDoItemDto.Title, returnedTask.Title);
            Assert.Equal(validToDoItemDto.Description, returnedTask.Description);
            Assert.Equal(validToDoItemDto.IsComplete, returnedTask.IsComplete);

            mockDbSet.Verify(m => m.Add(It.Is<ToDoItem>(t =>
                t.Title == validToDoItemDto.Title &&
                t.Description == validToDoItemDto.Description &&
                t.IsComplete == validToDoItemDto.IsComplete)), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }
        
        [Fact]
        public async Task PostTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();

            mockContext.Setup(c => c.ToDoItems).ReturnsDbSet(new List<ToDoItem>());

            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var inValidItem = new CreateToDoItemDto
            {
                Title = "",
                IsComplete = false
            };

            controller.ModelState.AddModelError("Title", "Title is required.");

            var result = await controller.CreateTodoItem(inValidItem);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }
    
        [Fact]
        public async Task UpdateTodoItem_UpdatesItem_WhenModelIsValid()
        {
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
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingItem);

            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.UpdateTodoItem(1, updatedItemDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTask = Assert.IsType<ToDoItemDto>(okResult.Value);
            Assert.Equal(updatedItemDto.Title, returnedTask.Title);
            Assert.Equal(updatedItemDto.Description, returnedTask.Description);
            Assert.Equal(updatedItemDto.IsComplete, returnedTask.IsComplete);

            Assert.Equal(updatedItemDto.Title, existingItem.Title);
            Assert.Equal(updatedItemDto.Description, existingItem.Description);
            Assert.Equal(updatedItemDto.IsComplete, existingItem.IsComplete);

            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task DeleteTodoItem_DeletesItem_WhenTaskExists()
        {
            var itemToDelete = new ToDoItem
            {
                Id = 1, Title = "OldTitle"
            };

            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(itemToDelete);

            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.DeleteTodoItem(1);

            Assert.IsType<NoContentResult>(result);
            mockDbSet.Verify(m => m.Remove(itemToDelete), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var updatedItemDto = new UpdateToDoItemDto
            {
                Title = "NewTitle", Description = "NewDesc", IsComplete = true
            };
            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((ToDoItem)null);

            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.UpdateTodoItem(1, updatedItemDto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Task not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_ReturnsBadRequest_WhenTitleAlreadyExists()
        {
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
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var newItem = new CreateToDoItemDto
            {
                Title = "TestTask", Description = "Desc2", IsComplete = true
            };

            var result = await controller.CreateTodoItem(newItem);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("A task with this title already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsBadRequest_WhenTitleAlreadyExists()
        {
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
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingTask);

            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var updateDto = new UpdateToDoItemDto
            {
                Title = "NewTitle", Description = "Any", IsComplete = true
            };

            var result = await controller.UpdateTodoItem(1, updateDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("A task with this title already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var listTasks = new List<ToDoItem>().AsQueryable();
            var mockDbSet = listTasks.BuildMockDbSet();
            mockDbSet.Setup(m => m.FindAsync(5)).ReturnsAsync((ToDoItem)null);

            var mockContext = new Mock<ITodoContext>();
            var mockCleaner = new Mock<IDatabaseCleaner>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);

            var result = await controller.DeleteTodoItem(5);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c=>c.ToDoItems).ReturnsDbSet(new List<ToDoItem>
            {
                new ToDoItem {Id = 1, Title = "TestTask", Description = "Desc1", IsComplete = false}
            });
            var mockCleaner = new Mock<IDatabaseCleaner>();
            var controller = new ToDoItemsController(mockContext.Object, mockCleaner.Object);
            
            var invalidDto = new UpdateToDoItemDto { Title = "", Description = "Desc", IsComplete = false };
            controller.ModelState.AddModelError("Title", "Title is required.");

            var result = await controller.UpdateTodoItem(1, invalidDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }    
    }
}