using api.Controllers;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
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

        public UserProjectsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<UserProjectsController>>();
            _controller = new UserProjectsController(_dbContext, _loggerMock.Object);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }

       [Fact]
        public async System.Threading.Tasks.Task AddUserToProject_ReturnsBadRequest_InvalidUserId()
        {
    
            var result = await _controller.AddUserToProject(0,1,1);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("UserID is required.", message);
        }        

        [Fact]
        public async System.Threading.Tasks.Task AddUserToProject_ReturnsBadRequest_InvalidProjectId()
        {    
            var result = await _controller.AddUserToProject(1,0,1);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("ProjectID is required.", message);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddUserToProject_ReturnsBadRequest_InvalidRoleId()
        {    
            var result = await _controller.AddUserToProject(1,1,0);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("RoleID is required.", message);
        }

        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenUserDoesNotExist()        
        {
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Testing for when user does not exist" });
            await  _dbContext.Roles.AddAsync(new Role { Id = 1, RoleName = "Role" });
            await _dbContext.SaveChangesAsync();            

            var result = await _controller.AddUserToProject(999, 1, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User does not exist", notFoundResult.Value);
        }
        
        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "Koki-98" });
            await _dbContext.Roles.AddAsync(new Role { Id = 1, RoleName = "Guest" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddUserToProject(1, 20, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project does not exist", notFoundResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenRoleDoesNotExist()        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "Koki-98" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenRoleDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.AddUserToProject(1, 1, 5);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Role does not exist", notFoundResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateUserRoleInProject_ShouldReturnOk_WhenUserAndRoleExist()
        { 
            var user = new User { Id = 1, GitHubId = "Koki-98" };
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
            Assert.Equal("User role updated successfully.", okResult.Value);
            
            var updatedUserProject = await _dbContext.UserProjects.FirstOrDefaultAsync(up => up.MemberId == 1 && up.ProjectId == 1);
            Assert.NotNull(updatedUserProject);
            Assert.Equal(2, updatedUserProject.RoleId);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {

            var result = await _controller.RemoveUserFromProject(0, 1);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("UserID is required.", badRequestResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnBadRequest_WhenProjectIdIsInvalid()
        {

            var result = await _controller.RemoveUserFromProject(1, 0);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProjectID is required.", badRequestResult.Value);
        }

         [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnNotFound_WhenUserDoesNotExist()
        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "Koki-98" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenUserDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveUserFromProject(2, 1);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User does not exist", notFoundResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
        
        {
            await _dbContext.Users.AddAsync(new User { Id = 1, GitHubId = "Koki-98" });
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "WhenUserDoesNotExist" });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveUserFromProject(1, 2);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project does not exist", notFoundResult.Value);
        }
    }
}
