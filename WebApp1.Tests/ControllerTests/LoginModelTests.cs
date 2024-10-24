using System.ComponentModel.DataAnnotations;
using WebApp1.Controllers.API;

namespace WebApp1.Tests.ControllerTests
{
    public class LoginModelTests
    {
        private readonly LoginModel _loginModel;

        public LoginModelTests()
        {
            _loginModel = new LoginModel();
        }

        [Fact]
        public void LoginModel_ValidFields_ReturnsNoValidationErrors()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = "Password123!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void LoginModel_EmptyEmail_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = string.Empty;
            _loginModel.Password = "Password123!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Email is required.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_InvalidEmailFormat_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "invalid-email-format";
            _loginModel.Password = "Password123!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Invalid email format.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_EmptyPassword_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = string.Empty;

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Password is required.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_PasswordWithoutUpperCase_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = "password123!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_PasswordWithoutLowerCase_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = "PASSWORD123!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_PasswordWithoutNumber_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = "Password!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.", validationResults.First().ErrorMessage);
        }

        [Fact]
        public void LoginModel_PasswordTooShort_ReturnsValidationError()
        {
            // Arrange
            _loginModel.Email = "test@test.com";
            _loginModel.Password = "P1!";

            // Act
            var validationResults = ValidateModel(_loginModel);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.", validationResults.First().ErrorMessage);
        }

        private List<ValidationResult> ValidateModel(LoginModel model)
        {
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
