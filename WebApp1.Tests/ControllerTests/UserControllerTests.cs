using Business.Exceptions;
using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Models;
using System.Security.Claims;
using WebApp1.Controllers.API;

namespace WebApp1.Tests.ControllerTests
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;
        private readonly ApplicationDbContext _dbContext;

        public UserControllerTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null,
                null,
                null,
                null
            );
            _userServiceMock = new Mock<IUserService>(MockBehavior.Loose);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new UserController(_userManagerMock.Object, _dbContext, _signInManagerMock.Object, _userServiceMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                    }))
                }
            };
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null,
                null,
                userValidators,
                passwordValidators,
                null,
                new IdentityErrorDescriber(),
                null,
                null
            );
        }

        [Fact]
        public async Task UpdateUser_ValidModel_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateModel = new UserUpdateModel { UserName = "TestUser", Email = "test@example.com" };

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "OriginalUser",
                Email = "original@example.com"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _userServiceMock.Setup(us => us.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UserUpdateModel>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateUser(updateModel);

            // Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async Task UpdatePassword_Valid_ReturnsNoContent()
        {
            // Arrange
            _userServiceMock.Setup(us => us.UpdatePasswordAsync(It.IsAny<Guid>(), "OldPassword", "NewPassword")).ReturnsAsync(true);

            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "OldPassword", NewPassword = "NewPassword" };

            // Act
            var result = await _controller.UpdatePassword(passwordUpdateModel);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetUserProfile_ValidUser_ReturnsOkWithProfile()
        {
            // Arrange
            var userProfile = new UserProfileModel { UserName = "TestUser", Email = "test@example.com" };
            _userServiceMock.Setup(us => us.GetUserProfileAsync(It.IsAny<Guid>())).ReturnsAsync(userProfile);

            // Act
            var result = await _controller.GetUserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(userProfile, okResult.Value);
        }

        [Fact]
        public async Task UpdatePassword_IncorrectOldPassword_ThrowsMyApplicationException()
        {
            // Arrange
            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "WrongPassword", NewPassword = "NewPassword" };
            _userServiceMock.Setup(us => us.UpdatePasswordAsync(It.IsAny<Guid>(), passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.InvalidData));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                async () => await _controller.UpdatePassword(passwordUpdateModel));

            // Assert
            Assert.Equal(ErrorStatus.InvalidData, exception.ErrorStatus);
        }


        [Fact]
        public async Task UpdatePassword_WeakNewPassword_ThrowsMyApplicationException()
        {
            // Arrange
            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "OldPassword", NewPassword = "123" };

            _userServiceMock
                .Setup(us => us.UpdatePasswordAsync(It.IsAny<Guid>(), passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.InvalidData, "Password does not meet complexity requirements."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                async () => await _controller.UpdatePassword(passwordUpdateModel));

            // Assert
            Assert.Equal("Password does not meet complexity requirements.", exception.Message);
            Assert.Equal(ErrorStatus.InvalidData, exception.ErrorStatus);
        }
    }
}
