﻿using Xunit;
using api.Controllers;
using api.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using System.Security.Claims;

namespace api.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly DbContextOptions<TickItDbContext> _dbContextOptions;
        private readonly TickItDbContext _dbContext;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TickItDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TickItDbContext(_dbContextOptions);

            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_dbContext, _loggerMock.Object);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUserById_ReturnsUser()
        {
            string githubID = "GitHub User 1";
            var user = new User { Id = 1, GitHubId = githubID };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetUserById(1);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);

            Assert.Equal(1, returnedUser.ID);
            Assert.Equal(githubID, returnedUser.GitHubID);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUserById_NotFound()
        {
            var user = new User { Id = 1, GitHubId = "GitHub User 1" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetUserById(2);

            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;

            Assert.Equal("User not found", response.message.ToString());
        }

        [Fact]
        public async System.Threading.Tasks.Task GetUserNotifications_ReturnsListOfNotifications()
        {
            Mock<HttpContext>  _mockHttpContext = new Mock<HttpContext>();
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

            var users = new List<User>
            {
                new() { Id = 1, GitHubId = "GitHub User 1" },
                new() { Id = 2, GitHubId = "GitHub User 2" }
            };
            
            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            var notifications = new List<Notification>
            {
                //user1 = 4 notifications; user2 = 3 notifications
                new() { Id = 1, UserId = 1, ProjectId = 1, TaskId = 1, IsRead = false, Message = "notification 1", NotificationTypeId = 2, CreatedAt = new DateTime(2025,02,20)},
                new() { Id = 2, UserId = 2, ProjectId = 1, TaskId = 2, IsRead = true, Message = "notification 2", NotificationTypeId = 1, CreatedAt = new DateTime(2025,02,20)},
                new() { Id = 3, UserId = 1, ProjectId = 1, TaskId = 1, IsRead = true, Message = "notification 3", NotificationTypeId = 3, CreatedAt = new DateTime(2025,02,24)},
                new() { Id = 4, UserId = 1, ProjectId = 2, TaskId = 12, IsRead = true, Message = "notification 4", NotificationTypeId = 2, CreatedAt = new DateTime(2025,02,25)},
                new() { Id = 5, UserId = 2, ProjectId = 1, TaskId = 2, IsRead = false, Message = "notification 5", NotificationTypeId = 2, CreatedAt = new DateTime(2025,02,26)},
                new() { Id = 6, UserId = 2, ProjectId = 3, TaskId = 1, IsRead = true, Message = "notification 6", NotificationTypeId = 1, CreatedAt = new DateTime(2025,02,27)},
                new() { Id = 7, UserId = 1, ProjectId = 1, TaskId = 1, IsRead = false, Message = "notification 7", NotificationTypeId = 2, CreatedAt = new DateTime(2025,02,27)}
            };

            await _dbContext.Notifications.AddRangeAsync(notifications);
            await _dbContext.SaveChangesAsync();

            //User 1 notifications
            var result1 = await _controller.GetUserNotifications();
            var okResult1 = Assert.IsType<OkObjectResult>(result1);
            var returnedNotifications1 = Assert.IsAssignableFrom<List<NotificationsDTO>>(okResult1.Value);
            
            Assert.Equal(4, returnedNotifications1.Count);

            var notification1 = returnedNotifications1[0];
            Assert.Equal(1, notification1.UserId);
            Assert.Equal("notification 1", notification1.Message);
            Assert.False(notification1.IsRead);
            Assert.Equal(1, notification1.TaskId);
            Assert.Equal(1, notification1.Id);

            var notification4 = returnedNotifications1[2];
            Assert.Equal("notification 4", notification4.Message);
            Assert.True(notification4.IsRead);
            Assert.Equal(12, notification4.TaskId);
            Assert.Equal(4, notification4.Id);
        }
    }
}
