using Xunit;
using ChatApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_Initialization_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";

            // Act
            var user = new User
            {
                Username = username,
                Email = email
            };

            // Assert
            Assert.True(user.Username == username, "Username should match input");
            Assert.Equal(email, user.Email);
            Assert.False(user.IsEmailVerified); // Assuming default is false
        }

        [Fact]
        public void User_DefaultConstructor_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            // We are simply instantiating the user to see what defaults it gets assigned.
            var user = new User();

            // Assert
            // Checking if the default values match what is defined in the User model.
            Assert.Equal(UserRole.User, user.Role);
            Assert.Equal(Gender.Other, user.Gender);
            Assert.False(user.IsEmailVerified);
            Assert.Empty(user.Username);
            Assert.Empty(user.Email);
            Assert.Empty(user.PasswordHash);
            Assert.Null(user.VerificationToken);
            
            // Dates can be tricky to test, but we can verify it was set very recently
            var timeDifference = DateTime.UtcNow - user.CreatedAt;
            Assert.True(timeDifference.TotalSeconds < 1, "CreatedAt should be set to current UTC time upon creation");
        }

        [Fact]
        public void User_Initialization_ShouldInitializeNavigationCollections()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            // Navigation properties (lists) should be initialized to empty lists, not null, to prevent NullReferenceExceptions
            Assert.NotNull(user.UserChatRooms);
            Assert.Empty(user.UserChatRooms);
            
            Assert.NotNull(user.Messages);
            Assert.Empty(user.Messages);
        }

        // ============================================================
        // TEST 4: Validation should fail when Username is empty
        // The [Required] attribute on Username should trigger an error.
        // ============================================================
        [Fact]
        public void User_Validation_ShouldFailWhenUsernameIsEmpty()
        {
            // Arrange - create a user with empty username (violates [Required])
            var user = new User
            {
                Username = "",
                Email = "valid@email.com",
                PasswordHash = "hashedpassword"
            };

            // We use .NET's built-in Validator to simulate what ASP.NET does
            // when it receives a POST request with model binding
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act - run validation
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert - should fail because Username is [Required]
            Assert.False(isValid, "Validation should fail when Username is empty");
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        // ============================================================
        // TEST 5: Validation should fail when Username exceeds 50 chars
        // The [MaxLength(50)] attribute on Username should catch this.
        // ============================================================
        [Fact]
        public void User_Validation_ShouldFailWhenUsernameExceedsMaxLength()
        {
            // Arrange - create a username that is 51 characters long
            var user = new User
            {
                Username = new string('A', 51),  // 51 chars > MaxLength(50)
                Email = "valid@email.com",
                PasswordHash = "hashedpassword"
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert - should fail because 51 > MaxLength of 50
            Assert.False(isValid, "Validation should fail when Username exceeds 50 characters");
        }

        // ============================================================
        // TEST 6: Validation should fail when Email format is invalid
        // The [EmailAddress] attribute checks for a valid email format.
        // ============================================================
        [Fact]
        public void User_Validation_ShouldFailWhenEmailFormatIsInvalid()
        {
            // Arrange - provide a badly formatted email
            var user = new User
            {
                Username = "testuser",
                Email = "not-a-valid-email",  // missing @ and domain
                PasswordHash = "hashedpassword"
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert - should fail because "not-a-valid-email" is not a valid email
            Assert.False(isValid, "Validation should fail when Email format is invalid");
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        // ============================================================
        // TEST 7: Validation should PASS with all valid data
        // A fully valid User should pass all validation checks.
        // ============================================================
        [Fact]
        public void User_Validation_ShouldPassWithValidData()
        {
            // Arrange - create a perfectly valid user
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashed_password_here"
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert - should pass with zero validation errors
            Assert.True(isValid, "Validation should pass for a fully valid User");
            Assert.Empty(validationResults);
        }
    }
}
