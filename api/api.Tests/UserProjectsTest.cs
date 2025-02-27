using api.Controllers;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests;


public class UserProjectsTest
{
    public class UserProjectsControllerTests
    {
        private Mock<ILogger<UserProjectsController>> _loggerMock;
        private DbContextOptions<TickItDbContext>? _dbContextOptions;
        private TickItDbContext? _dbContext;

        public UserProjectsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<UserProjectsController>>();
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }


        [Fact]
        public async System.Threading.Tasks.Task PostUserProjects_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            await _dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Testing for when user does not exist" });
            await  _dbContext.Roles.AddAsync(new Role { Id = 1, RoleName = "Role" });
            await _dbContext.SaveChangesAsync();
            var controller = new UserProjectsController(_dbContext, _loggerMock.Object);


            var result = await controller.AddUserToProject(999, 1, 1);


            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User does not exist", notFoundResult.Value);
        }
    }
}
