using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
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
        private readonly Mock<HttpContext> _mockHttpContext;

        public ProjectsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);
            _loggerMock = new Mock<ILogger<ProjectsController>>();
            _controller = new ProjectsController(_dbContext, _loggerMock.Object);
            _mockHttpContext = new Mock<HttpContext>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "GitHub User 1")
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var user = new ClaimsPrincipal(identity);

            _mockHttpContext.Setup(x => x.User).Returns(user);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
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
                new() { Id = 1, GitHubId = "GitHub User 1" },
                new() { Id = 2, GitHubId = "user456" }
            };

            string projectName1 = "project 1";
            string projectDescription1 = "project description for project 1";
            string projectName2 = "project 2";
            string projectDescription2 = "project description for project 2";
            var projects = new List<Project>
            {
                new()
                {
                    Id = 1,
                    OwnerId = 1,
                    ProjectName = projectName1,
                    ProjectDescription = projectDescription1,
                    UserProjects = new List<UserProject>
                    {
                        new UserProject { MemberId = 2, Member = users[1] }
                    }
                },

                new()
                {
                    Id = 2,
                    OwnerId = 2,
                    ProjectName = projectName2,
                    ProjectDescription = projectDescription2
                }
            };

            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

           
            var result1 = await _controller.GetProjectById(1);
            var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
            var returnedProject1 = Assert.IsType<ProjectWithTasksDTO>(okResult1.Value);
            var assignees1 = returnedProject1.AssignedUsers;

            Assert.Equal(1, returnedProject1.ID);
            Assert.Equal(projectName1, returnedProject1.ProjectName);
            Assert.Equal(projectDescription1, returnedProject1.ProjectDescription);
            Assert.Single(assignees1);
            Assert.Equal(2, assignees1[0].ID);
        }

        [Fact]
        public async systemTasks.Task GetProjectById_ReturnsProjectNotFound()
        {
            int id = 1;
            var result = await _controller.GetProjectById(id);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = notFoundResult.Value as dynamic;

            Assert.Equal($"Project with ID {id} not found", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task AddProject_ReturnsCreatedProjectDTO()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var projectDTO = new CreateProjectDTO
            {
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1",
            };

            var result = await _controller.AddProject(projectDTO);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProjectDTO = Assert.IsType<ProjectDTO>(createdResult.Value);

            //check created for the correct table
            Assert.Equal(projectDTO.ProjectName, createdProjectDTO.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, createdProjectDTO.ProjectDescription);

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
            CreateProjectDTO? projectDTO = null;
            var result = await _controller.AddProject(projectDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value as dynamic;
            Assert.Equal("Project data is null", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task AddProject_ReturnsConflict_ProjectAlreadyExists()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var projectDTO = new CreateProjectDTO
            {
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1",
            };

            var result = await _controller.AddProject(projectDTO);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProjectDTO = Assert.IsType<ProjectDTO>(createdResult.Value);

            //check created for the correct table
            Assert.Equal(projectDTO.ProjectName, createdProjectDTO.ProjectName);
            Assert.Equal(projectDTO.ProjectDescription, createdProjectDTO.ProjectDescription);

            result = await _controller.AddProject(projectDTO);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var response = conflictResult.Value as dynamic;

            Assert.Equal("A project with this name already exists for this owner", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task DeleteProject_ReturnsNoContent_DeleteProject()
        {
            var user = new User  { Id = 1, GitHubId = "GitHub User 1" };

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
            var userId = 1;
            var user = new User { Id = userId, GitHubId = "GitHub User 1" };
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                OwnerId = userId,
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.DeleteProject(projectId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal($"Project with ID {projectId} not found.", response.message.ToString());
        }

        
        [Fact]
        public async systemTasks.Task GetUsersProjects_ReturnsNotFound_UserDoesNotExist()
        {
            var result = await _controller.GetUsersProjects(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("User does not exist", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsBadRequest_NoLabelNameProvided()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var projectDTO = new CreateProjectDTO
            {
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1",
            };

            var resultNull = await _controller.AddProjectLabel(1, null);
            var badRequestResultNull = Assert.IsType<BadRequestObjectResult>(resultNull.Result);
            var responseNull = badRequestResultNull.Value as dynamic;
            Assert.Equal("Label name is required.", responseNull.message.ToString());

            var resultEmpty = await _controller.AddProjectLabel(1, "");
            var badRequestResultEmpty = Assert.IsType<BadRequestObjectResult>(resultEmpty.Result);
            var responseEmpty = badRequestResultEmpty.Value as dynamic;
            Assert.Equal("Label name is required.", responseEmpty.message.ToString());
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsBadRequest_InvalidProjectProvided()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();


            var result1 = await _controller.AddProjectLabel(0, "label 1");
            var badRequestResult1 = Assert.IsType<BadRequestObjectResult>(result1.Result);
            var response1 = badRequestResult1.Value as dynamic;
            Assert.Equal("Project ID is required.", response1.message.ToString());

            var result2 = await _controller.AddProjectLabel(-1, "label 1");
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(result2.Result);
            var response2 = badRequestResult2.Value as dynamic;
            Assert.Equal("Project ID is required.", response2.message.ToString());
        }

        [Fact]
        public async systemTasks.Task AddProjectLabel_ReturnsNotFound_ProjectDoesNotExist()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddProjectLabel(1, "label 1");
            var notFoundResult= Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("Project not found.", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task DeleteProjectLabel_ReturnsBadRequest_InvalidProjectProvided()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();


            var result1 = await _controller.DeleteProjectLabel(0, "label 1");
            var badRequestResult1 = Assert.IsType<BadRequestObjectResult>(result1);
            var response1 = badRequestResult1.Value as dynamic;
            Assert.Equal("Project ID is required.", response1.message.ToString());

            var result2 = await _controller.DeleteProjectLabel(-1, "label 1");
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(result2);
            var response2 = badRequestResult2.Value as dynamic;
            Assert.Equal("Project ID is required.", response2.message.ToString());
        }

        [Fact]
        public async systemTasks.Task DeleteProjectLabel_ReturnsNotFound_ProjectDoesNotExist()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
          
            var result = await _controller.DeleteProjectLabel(1, "label 1");
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("Project not found.", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task UpdateProject_ShouldReturnBadRequest_WhenProjectInvalid()
        {
            UpdateProjectDTO? updateProject = null;
            var result = await _controller.UpdateProject(0,updateProject);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("Invalid update data",value.message.ToString());
        }

        [Fact]
        public async systemTasks.Task UpdateTask_ReturnsNotFound_WhenProjectNotExist()
        {   
            var projectId = 1;
            var projectId2 = 2;
            var userId = 1;
            var user = new User
            {
                Id = userId,
                GitHubId = "GitHub User 1"
            };
            var project = new Project
            {
                Id = projectId,
                OwnerId = userId,
                ProjectName = "testing updateTask1",
                ProjectDescription = "project description for updateTask1"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            UpdateProjectDTO updateProject = new UpdateProjectDTO
            {
                ProjectName = "testing updateTask2",
                ProjectDescription = "project description for updateTask2"
            };
            
            var result = await _controller.UpdateProject(projectId2,updateProject);
            var notFoundResult= Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal($"Project with ID {projectId2} not found", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task UpdateTask_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            var userId = 1;
            var projectId = 1;
            var user = new User
            {
                Id = userId,
                GitHubId ="Unauthorized-user"
            };

             var project = new Project
            {
                Id = projectId,
                OwnerId = userId,
                ProjectName = "testing Unauthorized-user",
                ProjectDescription = "project description for Unauthorized-user"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            UpdateProjectDTO updateProject = new UpdateProjectDTO
            {
                ProjectName = "testing updateTask2",
                ProjectDescription = "project description for updateTask2"
            };
            
            var result = await _controller.UpdateProject(projectId,updateProject);
            var Unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Unauthorized.Value as dynamic;
            Assert.Equal("User not found", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task UpdateProject_Returns403_WhenUserLacksPermission()
        {
            var userId = 1;
            var ownerId = 2;
            var projectId = 1;

            var user = new User
            {
                Id = userId,
                GitHubId = "GitHub User 1"
            };

            var project = new Project
            {
                Id = projectId,
                OwnerId = ownerId,
                ProjectName = "Testing No Permission",
                ProjectDescription = "Project description for No Permission"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            var updateProjectDto = new UpdateProjectDTO
            {
                ProjectName = "Updated Name",
                ProjectDescription = "Updated Description"
            };

            
            var result = await _controller.UpdateProject(projectId, updateProjectDto);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value as dynamic;
            Assert.Equal("You don't have permission to modify this project", response.message.ToString());
        }

        [Fact]
        public async systemTasks.Task UpdateProject_ReturnsOk_WhenProjectSuccessfullyUpdated()
        {
           
            var userId = 1;
            var projectId = 1;

            var user = new User
            {
                Id = userId,
                GitHubId = "GitHub User 1"
            };

            var project = new Project
            {
                Id = projectId,
                OwnerId = userId,
                ProjectName = "Testing successfully updated",
                ProjectDescription = "Project description for successfully updated"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            var updateProjectDto = new UpdateProjectDTO
            {
                ProjectName = "Updated Name",
                ProjectDescription = "Updated Description"
            };

            
            var result = await _controller.UpdateProject(projectId, updateProjectDto);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProjectDTO>(okResult.Value);

            var returnedproject = response;
            Assert.Equal(returnedproject.ProjectName, response.ProjectName);
            Assert.Equal(returnedproject.ProjectDescription, response.ProjectDescription);
            Assert.Equal(userId, response.Owner.ID);
        }
    }
}
