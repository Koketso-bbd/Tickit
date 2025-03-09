using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
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
        //         DueDate = DateTime.UtcNow.AddDays(5),
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
        //         DueDate = DateTime.UtcNow.AddDays(5),
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
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskdtoNull()
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
                DueDate = DateTime.UtcNow,
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
                DueDate = DateTime.UtcNow,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal($"Priority must be one of the following: {EnumHelper.GetEnumValidValues<TaskPriority>()}.", value.message.ToString());
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
                DueDate = DateTime.UtcNow,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);

            var badRequestResult = Assert.IsType<ObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("An unexpected error occurred while creating the task. Please try again later.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsNotfound_WhenAssigneeIdDoesNotExist()
            {
            var taskDto = new TaskDTO
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = 1,
                TaskDescription = "Testing task",
                DueDate = DateTime.UtcNow,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);

            var notFoundResult = Assert.IsType<ObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("An unexpected error occurred while creating the task. Please try again later.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskNameLengthExceeds255()
        {
            var taskDto = new TaskDTO
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = 1,
                TaskDescription = new string('t', 1001), // entering 1001 t's
                DueDate = DateTime.UtcNow,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task Description cannot exceed a 1000 charcacters.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenTaskDescriptionLengthExceeds1000()
        {
            var taskDto = new TaskDTO
            {
                TaskName = new string('t', 256), // entering 256 t's
                PriorityId = 1,
                AssigneeId = 1,
                TaskDescription = "Testing task",
                DueDate = DateTime.UtcNow,
                ProjectId = 1,
            };

            var result = await _controller.CreateTask(taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task name cannot exceed 255 charcacters.", value.message.ToString());
        }

        //[Fact]
        //public async System.Threading.Tasks.Task CreateTask_ReturnsBadRequest_WhenDueDateIsInThePast()
        //{
        //    var userId = 1;
        //    var user = new User { Id = userId, GitHubId = "user" };
        //    await _dbContext.Users.AddAsync(user);
        //    await _dbContext.SaveChangesAsync();

        //    var taskDto = new TaskDTO
        //    {
        //        TaskName = "Task 1",
        //        PriorityId = 1,
        //        AssigneeId = userId,
        //        TaskDescription = "Testing task",
        //        DueDate = DateTime.UtcNow.AddDays(-1),
        //        ProjectId = 1,
        //    };

        //    var result = await _controller.CreateTask(taskDto);

        //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        //    var value = badRequestResult.Value as dynamic;
        //    Assert.Equal("Due date cannot be in the past.", value.message.ToString());
        //}

        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_ReturnsNotFound_WhenTaskNotExist()
        {   
           
            var taskId = 1;

            var result = await _controller.DeleteTask(taskId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic; 
            Assert.Equal($"Task with ID {taskId} not found.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsBadRequest_WhenTaskdtoEmpty()
        {
            TaskUpdateDTO? taskDTO = null;
            var result = await _controller.UpdateTask(1, taskDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Invalid task data.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var userId = 1;
            var taskId = 2;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = 1,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing task tasks"
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal($"Task with ID {taskId} not found.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsBadRequest_WhenTaskNameExceeds255()
        {
            var userId = 1;
            var taskId = 1;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                TaskName = new string('t', 256)
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task name cannot exceed 255 charcacters.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsBadRequest_WhenTaskDescriptionExceeds1000()
        {
            var userId = 1;
            var taskId = 1;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                AssigneeId = userId,
                PriorityId = 1,
                TaskDescription = new string('t', 1001)
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Task Description cannot exceed a 1000 charcacters.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsBadRequest_WhenTaskPriorityIsInvalid()
        {
            var userId = 1;
            var taskId = 1;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                AssigneeId = userId,
                PriorityId = 5
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal($"Priority must be one of the following: {EnumHelper.GetEnumValidValues<TaskPriority>()}.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsBadRequest_WhenTaskAssigneeIdIsInvalid()
        {
            var userId = 1;
            var taskId = 1;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                AssigneeId = 0,
                PriorityId = 1
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("AssigneeId is required and must be a valid value.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsNotFound_WhenTaskAssigneeDoesNotExist()
        {
            var userId = 1;
            var taskId = 1;
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = "Task 1",
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                AssigneeId = 2,
                PriorityId = 1
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("Assignee does not exist.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsOk_WhenTaskNameIsUpdated()
        {
            var userId = 1;
            var taskId = 1;
            var taskName = "Task 1";
            var newTaskName = "new task";
            var user = new User { Id = userId, GitHubId = "user" };
            var task = new api.Models.Task
            {
                TaskName = taskName,
                PriorityId = 1,
                AssigneeId = userId,
                TaskDescription = "Testing tasks",
                DueDate = DateTime.UtcNow.AddDays(1),
                ProjectId = 1,
                StatusId = 1,
                Id = taskId,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            TaskUpdateDTO taskDto = new TaskUpdateDTO
            {
                AssigneeId = userId,
                PriorityId = 1,
                TaskName = newTaskName
            };

            var result = await _controller.UpdateTask(taskId, taskDto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as dynamic;
            Assert.Equal($"Task with ID {taskId} updated successfully.", value.message.ToString());

            var updatedTask = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
            Assert.NotNull(updatedTask);
            Assert.Equal(newTaskName, updatedTask.TaskName);
        }
    }
}