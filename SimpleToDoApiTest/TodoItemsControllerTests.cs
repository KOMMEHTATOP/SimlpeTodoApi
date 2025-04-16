using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleToDoApi.Controllers;
using SimpleToDoApi.Data;
using SimpleToDoApi.Models;
using MockQueryable.Moq;

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
            var validToDoItem = new ToDoItem { Title = "TestTask", IsComplete = false };
            var mockDbSet = new Mock<DbSet<ToDoItem>>();
            var mockContext = new Mock<ITodoContext>();
            mockContext.Setup(c => c.ToDoItems).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new TodoItemsController(mockContext.Object);

            var result = controller.PostTodoItem(validToDoItem);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtResult.StatusCode);

            var returnedTask = Assert.IsType<ToDoItem>(createdAtResult.Value);
            Assert.Equal(validToDoItem.Title, returnedTask.Title);

            mockDbSet.Verify(m => m.Add(validToDoItem), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void PostTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var mockContext = new Mock<ITodoContext>(); 
            var controller = new TodoItemsController(mockContext.Object);

            // Имитируем ошибку валидации
            controller.ModelState.AddModelError("Title", "Required");
            var invalidItem = new ToDoItem { IsComplete = false }; 

            // Act
            var result = controller.PostTodoItem(invalidItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            // Проверяем, что вернулись ошибки валидации
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Title"));
        }

        //-------------------------UpdateTodoItem (PUT /update-task-{id})--------------------------

        [Fact]
        public void UpdateTodoItem_UpdatesItem_WhenModelIsValid()
        {
            var existingItem = new ToDoItem { Id=1, Title = "OldTitle" };
            var updatedItem = new ToDoItem { Id = 1, Title = "NewTitle" };
            
            var mockContext = new Mock<ITodoContext>();
            var controller = new TodoItemsController(mockContext.Object);

            mockContext.Setup(c => c.ToDoItems.Find(updatedItem.Id)).Returns(existingItem);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var result = controller.UpdateTodoItem(1, updatedItem);

            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
            Assert.Equal("NewTitle", existingItem.Title);

            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateTodoItem_ReturnsBadRequest_WhenModelIsInvalid()
        {
            var invalidItem = new ToDoItem { Id=1, Title = "" };
            var mockContext = new Mock<ITodoContext>();

            var controller = new TodoItemsController(mockContext.Object);
            controller.ModelState.AddModelError("Title", "Required");

            var result = controller.UpdateTodoItem(invalidItem.Id, invalidItem);

            var errorResponse = Assert.IsType<BadRequestObjectResult>(result);
            //проверка содержимого ошибки
            Assert.NotNull(errorResponse.Value); // Добавляем проверку на null
            var serializableError = Assert.IsType<SerializableError>(errorResponse.Value); // Явное приведение с проверкой
            Assert.True(serializableError.ContainsKey("Title"));        
        }

        [Fact]
        public void UpdateTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {

        }

        [Fact]
        public void UpdateTodoItem_ReturnsBadRequest_WhenIdMismatch() //(если id в URL ≠ id в теле запроса)
        {

        }

        [Fact]
        public void UpdateTodoItem_ReturnsForbidden_WhenNotAdmin()
        {

        }

        //-------------------------DeleteTodoItem (DELETE /delete-task-{id})--------------------------
        [Fact]
        public void DeleteTodoItem_DeletesItem_WhenTaskExists()
        {

        }

        [Fact]
        public void DeleteTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {

        }

        [Fact]
        public void DeleteTodoItem_ReturnsForbidden_WhenNotAdmin()
        {

        }


        //-------------------------DeleteAll (DELETE /delete-all)--------------------------

        [Fact]
        public void DeleteAll_ClearsAllItems_WhenAdminRole()
        {

        }

        [Fact]
        public void DeleteAll_ReturnsForbidden_WhenNotAdmin()
        {

        }
    }
}