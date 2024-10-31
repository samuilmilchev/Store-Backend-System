using Business.Intefraces;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Models;
using WebApp1.Controllers.API;
using WebApp1.Services;

namespace WebApp1.Tests.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
                Mock.Of<IRoleStore<ApplicationRole>>(), null, null, null, null);
            _mockConfiguration = new Mock<IConfiguration>();

            _mockUrlHelper = new Mock<IUrlHelper>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockAuthService = new Mock<IAuthService>();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser user, string password) =>
                    password.Any(char.IsUpper) ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Password must contain at least one uppercase letter." }));

            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser user, string password) =>
                {
                    bool hasUpperCase = password.Any(char.IsUpper);
                    bool hasLowerCase = password.Any(char.IsLower);
                    bool hasDigit = password.Any(char.IsDigit);
                    bool hasSpecialChar = password.Any(ch => @"@$!%*?&#".Contains(ch));
                    bool isValidLength = password.Length >= 8;

                    if (hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar && isValidLength)
                    {
                        return IdentityResult.Success;
                    }
                    else
                    {
                        var errors = new List<IdentityError>();

                        if (!hasUpperCase)
                            errors.Add(new IdentityError { Description = "Password must contain at least one uppercase letter." });
                        if (!hasLowerCase)
                            errors.Add(new IdentityError { Description = "Password must contain at least one lowercase letter." });
                        if (!hasDigit)
                            errors.Add(new IdentityError { Description = "Password must contain at least one number." });
                        if (!hasSpecialChar)
                            errors.Add(new IdentityError { Description = "Password must contain at least one special character." });
                        if (!isValidLength)
                            errors.Add(new IdentityError { Description = "Password must be at least 8 characters long." });

                        return IdentityResult.Failed(errors.ToArray());
                    }
                });

            _authController = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockRoleManager.Object,
            _mockConfiguration.Object,
            _mockEmailService.Object,
            _mockAuthService.Object);

            _authController.Url = _mockUrlHelper.Object;

            _mockHttpContext.Setup(c => c.Request.Scheme).Returns("https");

            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
        }


        [Fact]
        public async Task SignIn_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginModel = new LoginModel { Email = "test@test.com", Password = "Password123!" };
            var user = new ApplicationUser { Email = "test@test.com", UserName = "test@test.com" };

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(user, It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("your-secret-key");

            // Act
            var result = await _authController.SignIn(loginModel);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_ValidData_CreatesUserAndAssignsRole()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "Password123!" };
            var user = new ApplicationUser { Email = signUpRequest.Email, UserName = signUpRequest.Email };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync("User"))
                .ReturnsAsync(true);

            _mockEmailService.Setup(es => es.SendEmailConfirmation(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://test-confirmation-link");

            _mockAuthService.Setup(authService => authService.SignUpAsync(signUpRequest))
                .ReturnsAsync((true, user.Id.ToString(), "token", null));

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        public async Task SignUp_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _authController.ModelState.AddModelError("Email", "Invalid email format");
            var signUpRequest = new SignUpRequest { Email = "invalid", Password = "Password123!" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignIn_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginModel = new LoginModel { Email = "wrong@test.com", Password = "WrongPassword123!" };

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            _mockAuthService.Setup(auth => auth.SignInAsync(loginModel))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

            // Act
            var result = await _authController.SignIn(loginModel);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task SignUp_UserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "existing@test.com", Password = "Password123!" };
            var existingUser = new ApplicationUser { Email = signUpRequest.Email, UserName = signUpRequest.Email };

            _mockUserManager.Setup(um => um.FindByEmailAsync(signUpRequest.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidToken = "invalid-token";
            var user = new ApplicationUser { Id = userId, Email = "test@test.com" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.ConfirmEmailAsync(user, invalidToken))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            _mockAuthService.Setup(a => a.ConfirmEmailAsync(userId.ToString(), invalidToken))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            // Act
            var result = await _authController.ConfirmEmail(userId.ToString(), invalidToken);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_ValidToken_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var validToken = "valid-token";
            var user = new ApplicationUser { Id = userId, Email = "test@test.com" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.ConfirmEmailAsync(user, validToken))
                .ReturnsAsync(IdentityResult.Success);

            _mockAuthService.Setup(a => a.ConfirmEmailAsync(userId.ToString(), validToken))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.ConfirmEmail(userId.ToString(), validToken);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SignUp_RoleCreationFails_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "Password123!" };
            var user = new ApplicationUser { Email = signUpRequest.Email, UserName = signUpRequest.Email };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync("User"))
                .ReturnsAsync(false);
            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role creation failed." }));

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_InvalidEmailFormat_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "invalid-email-format", Password = "Password@123" };

            _authController.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var errorMessages = badRequestResult.Value as SerializableError;
            Assert.True(errorMessages.ContainsKey("Email"));
        }


        [Fact]
        public async Task SignUp_EmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "", Password = "Password123!" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_EmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_PasswordTooShort_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "short" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_PasswordWithoutUpperCase_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "password" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_PasswordWithoutLowerCase_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "PASSWORD123!" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_PasswordWithoutNumber_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "Password!" };

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_EmailServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var signUpRequest = new SignUpRequest { Email = "test@test.com", Password = "Password123!" };
            var user = new ApplicationUser { Email = signUpRequest.Email, UserName = signUpRequest.Email };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockRoleManager.Setup(rm => rm.RoleExistsAsync("User"))
                .ReturnsAsync(true);
            _mockEmailService.Setup(es => es.SendEmailConfirmation(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service failure"));

            // Act
            var result = await _authController.SignUp(signUpRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
