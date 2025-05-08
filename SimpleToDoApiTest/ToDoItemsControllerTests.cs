using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleToDoApi.Controllers;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApiTest
{
    public class ToDoItemsControllerTests
    {
        [Fact]
        public async Task GetTodoItem_ReturnsTodoItem_WhenTaskExists()
        {
            var expected = new ToDoItemDto { Id = 1, Title = "Test", Description = "Desc", IsComplete = false };
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.GetByIdToDo(1)).ReturnsAsync(new ToDoItemResult { Item = expected });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.GetTodoItemById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var item = Assert.IsType<ToDoItemDto>(ok.Value);
            Assert.Equal(expected.Title, item.Title);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.GetByIdToDo(It.IsAny<int>())).ReturnsAsync(new ToDoItemResult
            {
                Error = ToDoItemResult.ToDoItemError.ToDoNotFound
            });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.GetTodoItemById(42);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Task not found.", notFound.Value);
        }

        [Fact]
        public async Task CreateTodoItem_ReturnsCreatedAt_WhenValid()
        {
            var input = new CreateToDoItemDto { Title = "Test", Description = "Desc", IsComplete = false };
            var output = new ToDoItemDto { Id = 1, Title = "Test", Description = "Desc", IsComplete = false };
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.CreateToDo(input)).ReturnsAsync(new CreateToDoResult { CreatedItem = output });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.CreateTodoItem(input);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var item = Assert.IsType<ToDoItemDto>(created.Value);
            Assert.Equal("Test", item.Title);
        }

        [Fact]
        public async Task CreateTodoItem_ReturnsBadRequest_WhenTitleExists()
        {
            var input = new CreateToDoItemDto { Title = "Duplicate", Description = "Desc", IsComplete = false };
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.CreateToDo(It.IsAny<CreateToDoItemDto>())).ReturnsAsync(new CreateToDoResult
            {
                Error = CreateToDoResult.CreateToDoItemDtoError.TitleExists
            });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.CreateTodoItem(input);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Title already exists", badRequest.Value);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsUpdated_WhenValid()
        {
            var updateDto = new UpdateToDoItemDto { Title = "Updated", Description = "UpdatedDesc", IsComplete = true };
            var updatedDto = new ToDoItemDto { Id = 1, Title = "Updated", Description = "UpdatedDesc", IsComplete = true };

            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.UpdateToDo(1, updateDto)).ReturnsAsync(new UpdateToDoItemResult
            {
                UpdatedToDoItem = updatedDto
            });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.UpdateTodoItem(1, updateDto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var item = Assert.IsType<ToDoItemDto>(ok.Value);
            Assert.Equal("Updated", item.Title);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsNotFound_WhenItemMissing()
        {
            var dto = new UpdateToDoItemDto { Title = "X", Description = "Y", IsComplete = false };

            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.UpdateToDo(1, dto)).ReturnsAsync(new UpdateToDoItemResult
            {
                Error = UpdateToDoItemResult.UpdateTodoItemResult.ItemNotFound
            });

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.UpdateTodoItem(1, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Task not found.", notFound.Value);
        }

        [Fact]
        public async Task DeleteTodoItem_ReturnsNoContent_WhenDeleted()
        {
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.DeleteId(1)).ReturnsAsync(true);

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.DeleteTodoItem(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTodoItem_ReturnsNotFound_WhenNotFound()
        {
            var mockService = new Mock<IToDoService>();
            mockService.Setup(s => s.DeleteId(999)).ReturnsAsync(false);

            var controller = new ToDoItemsController(mockService.Object);

            var result = await controller.DeleteTodoItem(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found.", notFound.Value);
        }
    }
}
