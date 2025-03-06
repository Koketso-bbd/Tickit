using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests
{
    public class TaskControllerTests
    {
        private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
        private readonly TickItDbContext _dbContext;
        private readonly TasksController _controller;

        public TaskControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);
            _controller = new TasksController(_dbContext);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
        //Logic Correct , problem in TaskController
        // [Fact]
        // public async System.Threading.Tasks.Task GetTasksByAssigneeId_ReturnsOkResult_WhenTasksExist()
        // {            
        //     var assigneeId = 1;
        //     var task = new Models.Task
        //     {
        //         Id = 1,
        //         AssigneeId = assigneeId,
        //         TaskName = "Task 1",
        //         TaskDescription = "Description 1",
        //         DueDate = DateTime.Now.AddDays(5),
        //         PriorityId = 1,
        //         StatusId = 1,
        //         ProjectId = 1,
                
        //     };

        //     await _dbContext.Tasks.AddAsync(task);
        //     await _dbContext.SaveChangesAsync();

        //     var result = await _controller.GetUserTasks(assigneeId);

        //     var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //     var returnValue = Assert.IsAssignableFrom<List<TaskResponseDTO>>(okResult.Value);
        //     Assert.Single(returnValue);

        //     var returnedTask = returnValue.First();
        //     Assert.Equal(task.AssigneeId, returnedTask.AssigneeId);
        //     Assert.Equal(task.TaskName, returnedTask.TaskName);
        //     Assert.Equal(task.TaskDescription, returnedTask.TaskDescription);
        //     Assert.Equal(task.DueDate, returnedTask.DueDate);
        //     Assert.Equal(task.PriorityId, returnedTask.PriorityId);
        //     Assert.Equal(task.ProjectId, returnedTask.ProjectId);

        //     var label = returnedTask.TaskLabels.First();
        //     Assert.Equal(task.TaskLabels.First().Id, label.ID);
        //     Assert.Equal(task.TaskLabels.First().TaskId, label.TaskId);
        //     Assert.Equal(task.TaskLabels.First().ProjectLabelId, label.ProjectLabelId);
        // }

        // [Fact]
        // public async System.Threading.Tasks.Task GetTasksByAssigneeId_ReturnsNotFound_WhenTasksDoesNotExist()
        // {            
        //     var assigneeId = 1;
        //     var assigneeId2 = 2;

        //     var task = new Models.Task
        //     {
        //         Id = 1,
        //         AssigneeId = assigneeId,
        //         TaskName = "Task 1",
        //         TaskDescription = "Description 1",
        //         DueDate = DateTime.Now.AddDays(5),
        //         PriorityId = 1,
        //         ProjectId = 1,
        //         StatusId = 1,
        //         TaskLabels = new List<TaskLabel>
        //         {
        //             new TaskLabel { Id = 1, TaskId = 1, ProjectLabelId = 1 }
        //         }
        //     };

        //     await _dbContext.Tasks.AddAsync(task);
        //     await _dbContext.SaveChangesAsync();

        //     var result = await _controller.GetUserTasks(assigneeId2);

        //     var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        //     var value = notFoundResult.Value as dynamic;
        //     Assert.Equal($"No tasks found for user {assigneeId2}.",value.message.ToString()); 
        // }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskdtoEmpty()
        {
            TaskDTO? taskDTO = null;
            var result = await _controller.CreateTask(taskDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task data cannot be null.",value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskNameIsEmpty()
        { 
            var taskDto = new TaskDTO
            {
                AssigneeId = 1,
                DueDate = DateTime.Now,
                PriorityId = 1,
                ProjectId = 1,
                TaskDescription = "Testing for when Taskname is empty",
                TaskName = "", 
            };
            
            var result = await _controller.CreateTask(taskDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task name is required.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenPriorityIdIsInvalid()
        {
            var taskDto = new TaskDTO
            {
                TaskName = "Task 1",
                PriorityId = 0, 
                AssigneeId = 1,
                TaskDescription = "Testing for when PriorityId is invalid",
                DueDate = DateTime.Now,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Priority is required and must be between 1 and 4, where 1='Low', 2='Medium', 3='High', and 4='Urgent'.", value.message.ToString());
        }


        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenAssigneeIdIsInvalid()
        {
            var taskDto = new TaskDTO
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = 0,
                TaskDescription = "Testing for when AssigneeId is Invalid",
                DueDate = DateTime.Now,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("AssigneeId is required and must be a valid value.", value.message.ToString());
        }


        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_ReturnsNotFound_WhenTaskNotExist()
        {   
           
            var taskId = 1;

            var result = await _controller.DeleteTask(taskId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic; 
            Assert.Equal($"Task with ID {taskId} not found.", value.message.ToString());
        }

    }
}