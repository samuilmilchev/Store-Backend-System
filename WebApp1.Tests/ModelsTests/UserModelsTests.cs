using DAL.Entities;
using Shared;
using System.ComponentModel.DataAnnotations;

namespace WebApp1.Tests.ModelsTests
{
    public class UserModelsTests
    {
        [Fact]
        public void UserUpdateModel_ValidProperties_ReturnsNoValidationErrors()
        {
            // Arrange
            var model = new UserUpdateModel
            {
                UserName = "ValidUser",
                Email = "valid@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddress { AddressDelivery = "123 New St" }
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void UserUpdateModel_InvalidEmail_ReturnsValidationError()
        {
            // Arrange
            var model = new UserUpdateModel
            {
                UserName = "ValidUser",
                Email = "invalid-email",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddress { AddressDelivery = "123 New St" }
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Invalid email format.");
        }

        [Fact]
        public void UserUpdateModel_RequiredEmail_ReturnsValidationError()
        {
            // Arrange
            var model = new UserUpdateModel
            {
                UserName = "ValidUser",
                Email = null,
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddress { AddressDelivery = "123 New St" }
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void UserUpdateModel_EmptyEmail_ReturnsValidationError()
        {
            // Arrange
            var model = new UserUpdateModel
            {
                UserName = "ValidUser",
                Email = "",
                PhoneNumber = "1234567890",
                AddressDelivery = new UserAddress { AddressDelivery = "123 New St" }
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void UserUpdateModel_InvalidPhoneNumber_ReturnsValidationError()
        {
            // Arrange
            var model = new UserUpdateModel
            {
                UserName = "ValidUser",
                Email = "valid@example.com",
                PhoneNumber = "123",
                AddressDelivery = new UserAddress { AddressDelivery = "123 New St" }
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Phone number must be exactly 10 digits.");
        }

        [Fact]
        public void UserProfileModel_ValidProperties_ReturnsNoValidationErrors()
        {
            // Arrange
            var model = new UserProfileModel
            {
                UserName = "ValidUser",
                Email = "valid@example.com",
                PhoneNumber = "1234567890",
                AddressDelivery = "123 New St"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void PasswordUpdateModel_ValidProperties_ReturnsNoValidationErrors()
        {
            // Arrange
            var model = new PasswordUpdateModel
            {
                OldPassword = "OldPassword1!",
                NewPassword = "NewPassword1!"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void PasswordUpdateModel_InvalidNewPassword_ReturnsValidationError()
        {
            // Arrange
            var model = new PasswordUpdateModel
            {
                OldPassword = "OldPassword1!",
                NewPassword = "invalid"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        [Fact]
        public void PasswordUpdateModel_RequiredNewPassword_ReturnsValidationError()
        {
            // Arrange
            var model = new PasswordUpdateModel
            {
                OldPassword = "OldPassword1!",
                NewPassword = null
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Single(validationResults);
            Assert.Contains(validationResults, e => e.ErrorMessage == "Password is required.");
        }

        private static List<ValidationResult> ValidateModel<T>(T model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
