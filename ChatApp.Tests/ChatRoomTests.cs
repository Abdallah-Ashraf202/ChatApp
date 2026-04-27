using Xunit;
using ChatApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Tests
{
    public class ChatRoomTests
    {
        // ============================================================
        // TEST 1: Verify that properties are correctly assigned
        // We create a ChatRoom with specific values and check they stick.
        // ============================================================
        [Fact]
        public void ChatRoom_Initialization_ShouldSetPropertiesCorrectly()
        {
            // Arrange - prepare the data we want to assign
            var name = "General";
            var description = "Main chat room for everyone";
            var creatorId = 42;

            // Act - create the ChatRoom and assign the properties
            var room = new ChatRoom
            {
                Name = name,
                Description = description,
                CreatedByUserId = creatorId
            };

            // Assert - verify each property was stored correctly
            Assert.Equal(name, room.Name);
            Assert.Equal(description, room.Description);
            Assert.Equal(creatorId, room.CreatedByUserId);
        }

        // ============================================================
        // TEST 2: Verify default values when no data is provided
        // A brand-new ChatRoom should have sensible defaults.
        // ============================================================
        [Fact]
        public void ChatRoom_DefaultConstructor_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act - just create a blank ChatRoom
            var room = new ChatRoom();

            // Assert - check all the defaults defined in the model
            Assert.Equal(0, room.Id);                    // int defaults to 0
            Assert.Empty(room.Name);                     // defaults to string.Empty
            Assert.Null(room.Description);               // nullable string defaults to null
            Assert.Equal(0, room.CreatedByUserId);       // int defaults to 0

            // CreatedAt should be set to "right now" automatically
            var timeDifference = DateTime.UtcNow - room.CreatedAt;
            Assert.True(timeDifference.TotalSeconds < 1, "CreatedAt should be auto-set to current UTC time");
        }

        // ============================================================
        // TEST 3: Navigation collections should be initialized (not null)
        // This prevents NullReferenceException when accessing .Messages
        // or .UserChatRooms on a new ChatRoom object.
        // ============================================================
        [Fact]
        public void ChatRoom_Initialization_ShouldInitializeNavigationCollections()
        {
            // Arrange & Act
            var room = new ChatRoom();

            // Assert - collections exist but are empty
            Assert.NotNull(room.UserChatRooms);
            Assert.Empty(room.UserChatRooms);

            Assert.NotNull(room.Messages);
            Assert.Empty(room.Messages);
        }

        // ============================================================
        // TEST 4: Description is optional (nullable)
        // A ChatRoom should work fine without a description.
        // ============================================================
        [Fact]
        public void ChatRoom_Description_ShouldBeOptional()
        {
            // Arrange & Act - create room without setting Description
            var room = new ChatRoom
            {
                Name = "No Description Room",
                CreatedByUserId = 1
            };

            // Assert - Description should remain null and that's OK
            Assert.Null(room.Description);
        }

        // ============================================================
        // TEST 5: Validation should fail when Name is missing
        // The [Required] attribute on Name should trigger a validation error.
        // ============================================================
        [Fact]
        public void ChatRoom_Validation_ShouldFailWhenNameIsEmpty()
        {
            // Arrange - create a room with an empty name (violates [Required])
            var room = new ChatRoom { Name = "" };

            // We use .NET's built-in Validator to simulate what happens
            // when the model is validated (e.g., during a POST request)
            var validationContext = new ValidationContext(room);
            var validationResults = new List<ValidationResult>();

            // Act - run validation
            var isValid = Validator.TryValidateObject(room, validationContext, validationResults, true);

            // Assert - it should NOT be valid because Name is required
            Assert.False(isValid, "Validation should fail when Name is empty");
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        // ============================================================
        // TEST 6: Validation should fail when Name exceeds MaxLength
        // The [MaxLength(100)] attribute on Name should catch this.
        // ============================================================
        [Fact]
        public void ChatRoom_Validation_ShouldFailWhenNameExceedsMaxLength()
        {
            // Arrange - create a name that is 101 characters long
            var room = new ChatRoom { Name = new string('A', 101) };

            var validationContext = new ValidationContext(room);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(room, validationContext, validationResults, true);

            // Assert - should fail because 101 > MaxLength of 100
            Assert.False(isValid, "Validation should fail when Name exceeds 100 characters");
        }
    }
}
