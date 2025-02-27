using Xunit;
using api.Controllers;
using api.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;

namespace api.Tests
{
    public class UsersControllerTests
    {
        private Mock<ILogger<UsersController>> _loggerMock;
        private DbContextOptions<TickItDbContext> _dbContextOptions;
        private TickItDbContext _dbContext;

        public UsersControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<UsersController>>();
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUsers_ReturnsListOfUsers()
        {
            var controller = new UsersController(_dbContext, _loggerMock.Object);

            var users = new List<User>
            {
                new() { Id = 1, GitHubId = "GitHub User 1" },
                new() { Id = 2, GitHubId = "GitHub User 2" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            var result = await controller.GetUsers();

            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);

            Assert.Equal(2, returnedUsers.Count());
            Assert.Contains(returnedUsers, u => u.ID == 1 && u.GitHubID == "GitHub User 1");
            Assert.Contains(returnedUsers, u => u.ID == 2 && u.GitHubID == "GitHub User 2");
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUsers_NotFound()
        {
            var controller = new UsersController(_dbContext, _loggerMock.Object);

            var result = await controller.GetUsers();

            Assert.NotNull(result);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var message = Assert.IsType<string>(notFoundResult.Value);

            Assert.Equal("No users found.", message);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUserById_ReturnsUser()
        {
            var controller = new UsersController(_dbContext, _loggerMock.Object);

            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await controller.GetUserById(1);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);

            Assert.Equal(1, returnedUser.ID);
            Assert.Equal("GitHub User 1", returnedUser.GitHubID);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUserById_NotFound()
        {
            var controller = new UsersController(_dbContext, _loggerMock.Object);

            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await controller.GetUserById(2);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var message = Assert.IsType<string>(notFoundResult.Value);

            Assert.Equal("User not found", message);
        }
    }
}
