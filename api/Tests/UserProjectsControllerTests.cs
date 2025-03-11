using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;

namespace api.Tests;

public class UserProjectsTest
{
    public class UserProjectsControllerTests
    {
        private readonly Mock<ILogger<UserProjectsController>> _loggerMock;
        private readonly UserProjectsController _controller;
        private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
        private readonly TickItDbContext _dbContext;
        private readonly Mock<HttpContext> _mockHttpContext;

        public UserProjectsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<UserProjectsController>>();
            _controller = new UserProjectsController(_dbContext, _loggerMock.Object);
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
            _dbContext?.Dispose();
        }

        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenUserDoesNotExist()        
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Testing for when user does not exist" });
            await  _dbContext.Roles.AddAsync(new Role { Id = 1, RoleName = "Role" });
            await _dbContext.SaveChangesAsync();            

            var result = await _controller.AddUserToProject(999, 1, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("User does not exist", value.message.ToString());
        }        

        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "GitHub User 1" });
            await _dbContext.Roles.AddAsync(new Role { Id = 1, RoleName = "Guest" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddUserToProject(1, 20, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("Project does not exist", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenRoleDoesNotExist()        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "GitHub User 1" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenRoleDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddUserToProject(1, 1, 5);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("Role does not exist. Available roles are: ",value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task AddUserToProject_ReturnsStatusCode403_WhenUserIsNotAdminOrProjectOwner()
        {
            var userId = 1;
            var userId2 = 2;
            var user = new User { Id = userId, GitHubId = "GitHub User 1" };
            var user2 = new User { Id = userId2, GitHubId = "GitHub User 2" };

            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                OwnerId = 2,
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1"
            };

            var roleId = 2;
            var userProject = new UserProject
            {
                Id = 1,
                MemberId = userId,
                ProjectId = projectId,
                RoleId = roleId
            };

            var roles = new List<Role>
            {
                new() { Id = 1 , RoleName = "Admin"},
                new() { Id = 2 , RoleName = "Collaborator"},
                new() { Id = 3 , RoleName = "Viewer"},
            };

            await _dbContext.Roles.AddRangeAsync(roles);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(user2);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.UserProjects.AddAsync(userProject);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddUserToProject(userId2, projectId, roleId);
            var forbidenResult = Assert.IsType<ObjectResult>(result);
            var value = forbidenResult.Value as dynamic;
            Assert.Equal("You don't have permission to add users to this project", value.message.ToString());
            Assert.Equal(forbidenResult.StatusCode, 403);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateUserRoleInProject_ShouldReturnOk_WhenUserAndRoleExist()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            var project = new Project { Id = 1, ProjectName = "updating user role" };
            var oldRole = new Role { Id = 1, RoleName = "Old Role" };
            var newRole = new Role { Id = 2, RoleName = "New Role" };
            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            _dbContext.Roles.Add(oldRole);
            _dbContext.Roles.Add(newRole);
            _dbContext.UserProjects.Add(new UserProject { MemberId = 1, ProjectId = 1, RoleId = 1 });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.UpdateUserRole(1, 1, 2);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as dynamic;
            Assert.Equal("User role updated successfully.", value.message.ToString());

            var updatedUserProject = await _dbContext.UserProjects.FirstOrDefaultAsync(up => up.MemberId == 1 && up.ProjectId == 1);
            Assert.NotNull(updatedUserProject);
            Assert.Equal(2, updatedUserProject.RoleId);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {

            var result = await _controller.RemoveUserFromProject(0, 1);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("UserID is required.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnBadRequest_WhenProjectIdIsInvalid()
        {
            var result = await _controller.RemoveUserFromProject(1, 0);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value as dynamic;
            Assert.Equal("ProjectID is required.", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnNotFound_WhenUserDoesNotExist()        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "GitHub User 1" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenUserDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveUserFromProject(2, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("User does not exist", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnNotFound_WhenProjectDoesNotExist()        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "GitHub User 1" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenUserDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveUserFromProject(1, 2);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value as dynamic;
            Assert.Equal("Project does not exist", value.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserToProject_ReturnsStatusCode403_WhenUserIsNotAdminOrProjectOwner()
        {
            var userId = 1;
            var userId2 = 2;
            var user = new User { Id = userId, GitHubId = "GitHub User 1" };
            var user2 = new User { Id = userId2, GitHubId = "GitHub User 2" };

            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                OwnerId = 2,
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1"
            };

            var roleId = 2;
            var userProject = new UserProject
            {
                Id = 1,
                MemberId = userId,
                ProjectId = projectId,
                RoleId = roleId
            };

            var userProject2 = new UserProject
            {
                Id = 2,
                MemberId = userId2,
                ProjectId = projectId,
                RoleId = roleId
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(user2);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.UserProjects.AddAsync(userProject);
            await _dbContext.UserProjects.AddAsync(userProject2);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveUserFromProject(userId2, projectId);
            var forbidenResult = Assert.IsType<ObjectResult>(result);
            var value = forbidenResult.Value as dynamic;
            Assert.Equal("You do not have permission to remove users from this project", value.message.ToString());
            Assert.Equal(forbidenResult.StatusCode, 403);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserToProject_ReturnsUnauthorized()
        {
            var result = await _controller.RemoveUserFromProject(1, 1);
            var forbidenResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = forbidenResult.Value as dynamic;
            Assert.Equal("User not found.", value.message.ToString());
            Assert.Equal(forbidenResult.StatusCode, 401);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateUserRole_ReturnsStatusCode403_WhenUserIsNotAdminOrProjectOwner()
        {
            var userId = 1;
            var user = new User { Id = userId, GitHubId = "GitHub User 1" };

            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                OwnerId = 2,
                ProjectName = "project 1",
                ProjectDescription = "project description for project 1"
            };

            var roleId = 2;
            var userProject = new UserProject
            {
                Id = 1,
                MemberId = userId,
                ProjectId = projectId,
                RoleId = roleId
            };

            var roles = new List<Role>
            {
                new() { Id = 1 , RoleName = "Admin"},
                new() { Id = 2 , RoleName = "Collaborator"},
                new() { Id = 3 , RoleName = "Viewer"},
            };

            await _dbContext.Roles.AddRangeAsync(roles);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.UserProjects.AddAsync(userProject);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.UpdateUserRole(userId, projectId, 3);
            var forbidenResult = Assert.IsType<ObjectResult>(result);
            var value = forbidenResult.Value as dynamic;
            Assert.Equal("You do not have permission to modify this project", value.message.ToString());
            Assert.Equal(forbidenResult.StatusCode, 403);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateUserRole_ReturnsUnauthorized()
        {
            var result = await _controller.UpdateUserRole(1, 1, 1);
            var forbidenResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = forbidenResult.Value as dynamic;
            Assert.Equal("User not found in the system", value.message.ToString());
            Assert.Equal(forbidenResult.StatusCode, 401);
        }
    }
}
