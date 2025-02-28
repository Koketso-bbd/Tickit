using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using systemTasks = System.Threading.Tasks;

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
        public async systemTasks.Task GetProjectById_ReturnsProject()
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
        public async systemTasks.Task GetProjectById_ReturnsProjectNotFound()
        {
            int id = 1;
            var result = await _controller.GetProjectById(id);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var message = Assert.IsType<string>(notFoundResult.Value);

            Assert.Equal($"Project with ID {id} not found", message);
        }

        [Fact]
        public async systemTasks.Task AddProject_ReturnsCreatedProjectDTO()
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

            //check created for the correct table
            Assert.Equal(projectDTO.ProjectName, createdProjectDTO.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, createdProjectDTO.ProjectDescription);
            Assert.Equal(projectDTO.Owner.ID, createdProjectDTO.Owner.ID);

            //check created project is stored in database
            var saveProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == createdProjectDTO.ID);
            Assert.NotNull(saveProject);
            Assert.Equal(projectDTO.ProjectName, saveProject.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, saveProject.ProjectDescription);
        }

        [Fact]
        public async systemTasks.Task AddProject_ReturnsBadRequest_InvalidProjectData()
        {
            ProjectDTO? projectDTO = null;
            var result = await _controller.AddProject(projectDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var message = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Project data is null.", message);
        }

        [Fact]
        public async systemTasks.Task AddProject_ReturnsConflict_ProjectAlreadyExists()
        {
            var projectDTO = new ProjectDTO
            {
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1",
                Owner = new UserDTO { ID = 1, GitHubID = "GitHub User 1" }
            };

            var result = await _controller.AddProject(projectDTO);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProjectDTO = Assert.IsType<ProjectDTO>(createdResult.Value);

            //check created for the correct table
            Assert.Equal(projectDTO.ProjectName, createdProjectDTO.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, createdProjectDTO.ProjectDescription);
            Assert.Equal(projectDTO.Owner.ID, createdProjectDTO.Owner.ID);

            result = await _controller.AddProject(projectDTO);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var message = Assert.IsType<string>(conflictResult.Value);

            Assert.Equal("A project with this name already exists for this owner", message);
        }

        [Fact]
        public async systemTasks.Task DeleteProject_ReturnsNoContent_DeleteProject()
        {
            var user = new User  { Id = 1, GitHubId = "owner123" };

            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                OwnerId = 1,
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1"
            };

            await _dbContext.Projects.AddAsync(project);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.DeleteProject(projectId);
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);

            //verify project no longer exists
            var deletedProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.Null(deletedProject);
        }

        [Fact]
        public async systemTasks.Task DeleteProject_ReturnsNotFound_ProjectDoesNotExist()
        {
            var result = await _controller.DeleteProject(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var message = Assert.IsType<string>(notFoundResult.Value);
            Assert.Equal("Project with ID 1 not found.", message);
        }

        [Fact]
        public async systemTasks.Task GetUsersProjects_ReturnsOk_ListOfProjects()
        {
            int userId = 1;
            User user = new User { Id = userId, GitHubId = "Github User 1" };
            var projects = new List<Project>
            {
                new()
                {
                    Id = 1,
                    OwnerId = userId,
                    ProjectName = "project 1",
                    ProjectDescription = "project description for project 1",
                },

                new()
                {
                    Id = 2,
                    OwnerId = userId,
                    ProjectName = "project 2",
                    ProjectDescription = "project description for project 2"
                }
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetUsersProjects(userId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProjects = Assert.IsAssignableFrom<IEnumerable<ProjectDTO>>(okResult.Value);

            Assert.Equal(2, returnedProjects.Count());
            Assert.Contains(returnedProjects, p => p.ID == 1 && p.ProjectName == "project 1" && p.ProjectDescription == "project description for project 1" && p.Owner.ID == userId);
            Assert.Contains(returnedProjects, p => p.ID == 2 && p.ProjectName == "project 2" && p.ProjectDescription == "project description for project 2" && p.Owner.ID == userId);
        }

        [Fact]
        public async systemTasks.Task GetUsersProjects_ReturnsNotFound_UserDoesNotExist()
        {
            var result = await _controller.GetUsersProjects(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var message = Assert.IsType<string>(notFoundResult.Value);
            Assert.Equal("User does not exist", message);
        }

        [Fact]
        public async systemTasks.Task GetProjectLabels_ReturnListOfProjectLabels()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            var projects = new List<Project>
            {
                new()
                {
                    Id = 1,
                    OwnerId = 1,
                    ProjectName = "project 1",
                    ProjectDescription = "project description for project 1",
                },

                new()
                {
                    Id = 2,
                    OwnerId = 1,
                    ProjectName = "project 2",
                    ProjectDescription = "project description for project 2"
                }
            };
            var labels = new List<Label>
            {
                new() { Id = 1, LabelName = "label 1"},
                new() { Id = 2, LabelName = "label 2"},
                new() { Id = 3, LabelName = "label 3"},
            };
            var projectLabels = new List<ProjectLabel>
            {
                new() { Id = 1, LabelId = 1, ProjectId = 1},
                new() { Id = 2, LabelId = 3, ProjectId = 1},
                new() { Id = 3, LabelId = 1, ProjectId = 2},
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.Labels.AddRangeAsync(labels);
            await _dbContext.ProjectLabels.AddRangeAsync(projectLabels);
            await _dbContext.SaveChangesAsync();

            var result1 = await _controller.GetProjectLabels(1);
            var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
            var returnedProjectLabels1 = Assert.IsType<List<ProjectLabelDTO>>(okResult1.Value);

            Assert.Equal(2, returnedProjectLabels1.Count());
            Assert.Contains(returnedProjectLabels1, pl => pl.ID == 1 && pl.LabelID == 1 && pl.ProjectID == 1);
            Assert.Contains(returnedProjectLabels1, pl => pl.ID == 2 && pl.LabelID == 3 && pl.ProjectID == 1);

            var result2 = await _controller.GetProjectLabels(2);
            var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
            var returnedProjectLabels2 = Assert.IsType<List<ProjectLabelDTO>>(okResult2.Value);

            Assert.Single(returnedProjectLabels2);
            Assert.Contains(returnedProjectLabels2, pl => pl.ID == 3 && pl.LabelID == 1 && pl.ProjectID == 2);
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsBadRequest_NoLabelNameProvided()
        {
            var resultNull = await _controller.AddProjectLabel(1, null);
            var badRequestResultNull = Assert.IsType<BadRequestObjectResult>(resultNull.Result);
            var messageNull = Assert.IsType<string>(badRequestResultNull.Value);
            Assert.Equal("labelName is required.", messageNull);

            var resultEmpty = await _controller.AddProjectLabel(1, "");
            var badRequestResultEmpty = Assert.IsType<BadRequestObjectResult>(resultEmpty.Result);
            var messageEmpty = Assert.IsType<string>(badRequestResultEmpty.Value);
            Assert.Equal("labelName is required.", messageEmpty);
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsBadRequest_InvalidProjectProvided()
        {
            var result1 = await _controller.AddProjectLabel(0, "label 1");
            var badRequestResult1 = Assert.IsType<BadRequestObjectResult>(result1.Result);
            var message1 = Assert.IsType<string>(badRequestResult1.Value);
            Assert.Equal("ProjectID is required.", message1);

            var result2 = await _controller.AddProjectLabel(-1, "label 1");
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(result2.Result);
            var message2 = Assert.IsType<string>(badRequestResult2.Value);
            Assert.Equal("ProjectID is required.", message2);
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsNotFound_ProjectDoesNotExist()
        {
            var result = await _controller.AddProjectLabel(1, "label 1");
            var notFoundResult= Assert.IsType<NotFoundObjectResult>(result.Result);
            var message= Assert.IsType<string>(notFoundResult.Value);
            Assert.Equal("Project not found", message);
        }

        [Fact]
        public async systemTasks.Task DeleteProjectLabel_ReturnsBadRequest_NoLabelNameProvided()
        {
            var resultNull = await _controller.DeleteProjectLabel(1, null);
            var badRequestResultNull = Assert.IsType<BadRequestObjectResult>(resultNull);
            var messageNull = Assert.IsType<string>(badRequestResultNull.Value);
            Assert.Equal("labelName is required.", messageNull);

            var resultEmpty = await _controller.DeleteProjectLabel(1, "");
            var badRequestResultEmpty = Assert.IsType<BadRequestObjectResult>(resultEmpty);
            var messageEmpty = Assert.IsType<string>(badRequestResultEmpty.Value);
            Assert.Equal("labelName is required.", messageEmpty);
        }
    }
}
