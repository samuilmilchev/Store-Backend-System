using AutoMapper;
using Business.Exceptions;
using Business.Services;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using Moq;
using Shared.DTOs;
using Shared.Models;

namespace WebApp1.Tests.ControllerTests
{
    public class UserServiceTests
    {
        private readonly UserService _userService;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_userManagerMock.Object, _dbContext, _mapperMock.Object);
        }

        [Fact]
        public async Task UpdateUserAsync_UserExists_UpdatesUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateModel = new UserUpdateModel
            {
                UserName = "UpdatedUser",
                Email = "updated@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddressDTO { AddressDelivery = "123 New St" }
            };

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "OriginalUser",
                Email = "original@example.com",
                PhoneNumber = "0987654321",
                AddressDelivery = new UserAddress { AddressDelivery = "456 Old St" }
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _userManagerMock.Setup(m => m.Users).Returns(_dbContext.Users);
            _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            _mapperMock.Setup(m => m.Map(updateModel, user)).Callback<UserUpdateModel, ApplicationUser>((src, dest) =>
            {
                dest.UserName = src.UserName;
                dest.Email = src.Email;
                dest.PhoneNumber = src.PhoneNumber;
                dest.AddressDelivery = new UserAddress { AddressDelivery = src.AddressDelivery.AddressDelivery };
            });

            // Act
            await _userService.UpdateUserAsync(userId, updateModel);

            // Assert
            Assert.Equal("UpdatedUser", user.UserName);
            Assert.Equal("updated@example.com", user.Email);
            Assert.Equal("1234567890", user.PhoneNumber);
            Assert.Equal("123 New St", user.AddressDelivery.AddressDelivery);
            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_UserExists_ChangesPassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldPassword = "OldPassword1!";
            var newPassword = "NewPassword1!";
            var user = new ApplicationUser { Id = userId };

            _userManagerMock.Setup(m => m.Users)
                .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, oldPassword, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdatePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(m => m.ChangePasswordAsync(user, oldPassword, newPassword), Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_UserExists_ReturnsUserProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "TestUser",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddress { AddressDelivery = "Test Address" }
            };

            var expectedProfile = new UserProfileModel
            {
                UserName = "TestUser",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = "Test Address"
            };

            _userManagerMock.Setup(m => m.Users)
                .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<UserProfileModel>(user)).Returns(expectedProfile);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.UserName);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("1234567890", result.PhoneNumber);
            Assert.Equal("Test Address", result.AddressDelivery);
        }

        [Fact]
        public async Task UpdateUserAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateModel = new UserUpdateModel
            {
                UserName = "UpdatedUser",
                Email = "updated@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddressDTO { AddressDelivery = "123 New St" }
            };

            _userManagerMock.Setup(m => m.Users)
                .Returns(new List<ApplicationUser>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() => _userService.UpdateUserAsync(userId, updateModel));
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_InvalidEmail_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "OriginalUser",
                Email = "original@example.com"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var updateModel = new UserUpdateModel
            {
                UserName = "UpdatedUser",
                Email = "invalid-email",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddressDTO { AddressDelivery = "123 New St" }
            };

            _userManagerMock.Setup(m => m.Users).Returns(_dbContext.Users);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _userService.UpdateUserAsync(userId, updateModel));
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePasswordAsync_IncorrectOldPassword_ThrowsMyApplicationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId };
            var oldPassword = "OldPassword1!";
            var newPassword = "NewPassword1!";

            _userManagerMock.Setup(m => m.Users)
                .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMock());

            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, oldPassword, newPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                async () => await _userService.UpdatePasswordAsync(userId, oldPassword, newPassword));

            Assert.Equal(ErrorStatus.InvalidData, exception.ErrorStatus);
            _userManagerMock.Verify(m => m.ChangePasswordAsync(user, oldPassword, newPassword), Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userManagerMock.Setup(m => m.Users).Returns(_dbContext.Users);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() => _userService.GetUserProfileAsync(userId));
        }
    }
}
