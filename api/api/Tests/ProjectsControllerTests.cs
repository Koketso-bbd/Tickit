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
    public class ProjectsControllerTests
    {
        private readonly Mock<ILogger<ProjectsController>> _loggerMock;
        private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
        private readonly TickItDbContext _dbContext;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);
            _loggerMock = new Mock<ILogger<ProjectsController>>();
            _controller = new ProjectsController(_dbContext, _loggerMock.Object);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async System.Threading.Tasks.Task GetProjectById_ReturnsProject()
        {
            var users = new List<User>
            {
                new() { Id = 1, GitHubId = "owner123" },
                new() { Id = 2, GitHubId = "user456" }
            };

            var projects = new List<Project>
            {
                new()
                {
                    Id = 1,
                    OwnerId = 1,
                    ProjectName = "project 1",
                    ProjectDescription = "project description for project 1",
                    UserProjects = new List<UserProject>
                    {
                        new UserProject { MemberId = 2, Member = users[1] }
                    }
                },

                new()
                {
                    Id = 2,
                    OwnerId = 2,
                    ProjectName = "project 2",
                    ProjectDescription = "project description for project 2"
                }
            };

            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            //project 1
            var result1 = await _controller.GetProjectById(1);
            var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
            var returnedProject1 = Assert.IsType<ProjectDTO>(okResult1.Value);
            var assignees1 = returnedProject1.AssignedUsers;

            Assert.Equal(1, returnedProject1.ID);
            Assert.Equal("project 1", returnedProject1.ProjectName);
            Assert.Equal("project description for project 1", returnedProject1.ProjectDescription);
            Assert.Single(assignees1);
            Assert.Equal(2, assignees1[0].ID);

            //project 2
            var result2 = await _controller.GetProjectById(2);
            var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
            var returnedProject2 = Assert.IsType<ProjectDTO>(okResult2.Value);
            var assignees2 = returnedProject2.AssignedUsers;

            Assert.Equal(2, returnedProject2.ID);
            Assert.Equal("project 2", returnedProject2.ProjectName);
            Assert.Equal("project description for project 2", returnedProject2.ProjectDescription);
            Assert.Empty(assignees2);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetProjectById_ReturnsProjectNotFound()
        {
            int id = 1;
            var result = await _controller.GetProjectById(id);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var message = Assert.IsType<string>(notFoundResult.Value);

            Assert.Equal($"Project with ID {id} not found", message);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddProject_ReturnsCreatedProjectDTO()
        {
            var projectDTO = new ProjectDTO
            {
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1",
                Owner = new UserDTO { ID = 1, GitHubID = "GitHub User 1"}
            };

            var result = await _controller.AddProject(projectDTO);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProjectDTO = Assert.IsType<ProjectDTO>(createdResult.Value);

            Assert.Equal(projectDTO.ProjectName, createdProjectDTO.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, createdProjectDTO.ProjectDescription);
            Assert.Equal(projectDTO.Owner.ID, createdProjectDTO.Owner.ID);

            var saveProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == createdProjectDTO.ID);
            Assert.NotNull(saveProject);
            Assert.Equal(projectDTO.ProjectName, saveProject.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, saveProject.ProjectDescription);
        }
    }
}
