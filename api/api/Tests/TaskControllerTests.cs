using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using api.Controllers;
using api.Data;
using api.DTOs;


namespace api.Tests;
public class TasksControllerTests
{
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
    private readonly TickItDbContext _dbContext;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        _dbContext = new TickItDbContext(_dbContextOptions);

        _loggerMock = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_dbContext, _loggerMock.Object);

    }

    [Fact]
    public void Dispose()
    {
        _dbContext?.Dispose();
    }


    [Fact]
    public async Task GetTasksInProject_ReturnsOk_WhenTasksExist()
    {
        var tasks = new TaskDTO

        {
            Id = 1,
            AssigneeId = 1,
            TaskName = "Task 1",
            TaskDescription = "Description 1",
            DueDate = DateTime.UtcNow,
            PriorityId = 1,
            ProjectId = 1,
            StatusId = 1,
        };
            
        await _dbContext.AddAsync(tasks);
        await _dbContext.SaveChangesAsync();

        var controller = new TasksController(_dbContext, _loggerMock.Object);


        var result = await controller.GetTasksInProject(1);

        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsAssignableFrom<IEnumerable<TaskDTO>>(okResult.Value);
        Assert.Equal(1, task.Count());
    }


    [Fact]
    public async Task GetTasksInProject_ReturnsNotFound_WhenNoTasksExist()
    {

        
        var controller = new TasksController(_dbContext, _loggerMock.Object);


        var result = await controller.GetTasksInProject(4);


        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No tasks found for Project ID 4.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetTasksInProject_ReturnsInternalServerError_OnException()
    {

        var mockContext = new Mock<TickItDbContext>(_dbContextOptions);
        mockContext.Setup(c => c.Tasks).Throws(new Exception("Database error"));
        var controller = new TasksController(mockContext.Object, _loggerMock.Object);


        var result = await controller.GetTasksInProject(1);


        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}
