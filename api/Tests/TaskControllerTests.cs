using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace api.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<ILogger<TasksController>> _loggerMock;
        private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
        private readonly TickItDbContext _dbContext;
        private readonly TasksController _controller;

        public TaskControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<TasksController>>();
            _controller = new TasksController(_dbContext);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }


        [Fact]
        public async System.Threading.Tasks.Task GetTasksByAssigneeId_ReturnsOkResult_WhenTasksExist()
        {
            
            var assigneeId = 1;

            var task = new Models.Task
            {
                Id = 1,
                AssigneeId = assigneeId,
                TaskName = "Task 1",
                TaskDescription = "Description 1",
                DueDate = DateTime.Now.AddDays(5),
                PriorityId = 1,
                ProjectId = 1,
                StatusId = 1,
                TaskLabels = new List<TaskLabel>
                {
                    new TaskLabel { Id = 1, TaskId = 1, ProjectLabelId = 1 }
                }
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            
            var result = await _controller. GetUserTasks(assigneeId);

            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<List<TaskDTO>>(okResult.Value);
            Assert.Single(returnValue);

            var returnedTask = returnValue.First();
            Assert.Equal(task.Id, returnedTask.Id);
            Assert.Equal(task.AssigneeId, returnedTask.AssigneeId);
            Assert.Equal(task.TaskName, returnedTask.TaskName);
            Assert.Equal(task.TaskDescription, returnedTask.TaskDescription);
            Assert.Equal(task.DueDate, returnedTask.DueDate);
            Assert.Equal(task.PriorityId, returnedTask.PriorityId);
            Assert.Equal(task.ProjectId, returnedTask.ProjectId);
            Assert.Equal(task.StatusId, returnedTask.StatusId);

            var label = returnedTask.TaskLabels.First();
            Assert.Equal(task.TaskLabels.First().Id, label.ID);
            Assert.Equal(task.TaskLabels.First().TaskId, label.TaskId);
            Assert.Equal(task.TaskLabels.First().ProjectLabelId, label.ProjectLabelId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByAssigneeId_ReturnsNotFound_WhenTasksDoesNotExist()
        {
            
            var assigneeId = 1;
            var assigneeId2 = 2;

            var task = new Models.Task
            {
                Id = 1,
                AssigneeId = assigneeId,
                TaskName = "Task 1",
                TaskDescription = "Description 1",
                DueDate = DateTime.Now.AddDays(5),
                PriorityId = 1,
                ProjectId = 1,
                StatusId = 1,
                TaskLabels = new List<TaskLabel>
                {
                    new TaskLabel { Id = 1, TaskId = 1, ProjectLabelId = 1 }
                }
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            
            var result = await _controller. GetUserTasks(assigneeId2);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var message = Assert.IsAssignableFrom<string>(notFoundResult.Value);
            Assert.Equal($"No tasks found for user {assigneeId2}.",message);

            
        }


        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskdtoEmpty()
        {
            TaskDTO? taskDTO = null;
            var result = await _controller.CreateTask(taskDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Task data cannot be null.",message);
        }
    }
}