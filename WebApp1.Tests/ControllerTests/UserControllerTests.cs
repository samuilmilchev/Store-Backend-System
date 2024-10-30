using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared;
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
            _userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

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
            var updateModel = new UserUpdateModel { UserName = "TestUser", Email = "test@example.com" };
            _userServiceMock.Setup(us => us.UpdateUserAsync(It.IsAny<Guid>(), updateModel)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(updateModel);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateUser_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required.");

            // Act
            var result = await _controller.UpdateUser(new UserUpdateModel());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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
        public async Task UpdateUser_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var updateModel = new UserUpdateModel { UserName = "NonExistentUser", Email = "nonexistent@example.com" };
            _userServiceMock.Setup(us => us.UpdateUserAsync(It.IsAny<Guid>(), updateModel)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUser(updateModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_IncorrectOldPassword_ReturnsBadRequest()
        {
            // Arrange
            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "WrongPassword", NewPassword = "NewPassword" };
            _userServiceMock.Setup(us => us.UpdatePasswordAsync(It.IsAny<Guid>(), passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdatePassword(passwordUpdateModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProfile_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            _userServiceMock.Setup(us => us.GetUserProfileAsync(It.IsAny<Guid>())).ReturnsAsync((UserProfileModel)null);

            // Act
            var result = await _controller.GetUserProfile();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_InvalidEmailFormat_ReturnsBadRequest()
        {
            // Arrange
            var updateModel = new UserUpdateModel { UserName = "TestUser", Email = "invalidemail" };
            _controller.ModelState.AddModelError("Email", "Invalid email format.");

            // Act
            var result = await _controller.UpdateUser(updateModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_WeakNewPassword_ReturnsBadRequest()
        {
            // Arrange
            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "OldPassword", NewPassword = "123" };

            _userServiceMock
                .Setup(us => us.UpdatePasswordAsync(It.IsAny<Guid>(), passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword))
                .ReturnsAsync(false);

            _controller.ModelState.AddModelError("NewPassword", "Password does not meet complexity requirements.");

            // Act
            var result = await _controller.UpdatePassword(passwordUpdateModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProfile_ValidUserNullProfile_ReturnsNoContent()
        {
            // Arrange
            _userServiceMock.Setup(us => us.GetUserProfileAsync(It.IsAny<Guid>())).ReturnsAsync((UserProfileModel)null);

            // Act
            var result = await _controller.GetUserProfile();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_NullModel_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateUser(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_SameOldAndNewPassword_ReturnsBadRequest()
        {
            // Arrange
            var passwordUpdateModel = new PasswordUpdateModel { OldPassword = "SamePassword", NewPassword = "SamePassword" };
            _controller.ModelState.AddModelError("NewPassword", "New password cannot be the same as the old password.");

            // Act
            var result = await _controller.UpdatePassword(passwordUpdateModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProfile_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.GetUserProfile();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
