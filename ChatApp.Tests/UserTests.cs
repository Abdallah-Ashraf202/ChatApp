using Xunit;
using ChatApp.Models;

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
    }
}
