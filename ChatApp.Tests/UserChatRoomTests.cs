using Xunit;
using ChatApp.Models;

namespace ChatApp.Tests
{
    public class UserChatRoomTests
    {
        // ============================================================
        // TEST 1: Verify that properties are correctly assigned
        // UserChatRoom is a join table linking Users to ChatRooms.
        // ============================================================
        [Fact]
        public void UserChatRoom_Initialization_ShouldSetPropertiesCorrectly()
        {
            // Arrange - prepare the foreign key values
            var userId = 3;
            var chatRoomId = 7;

            // Act - create the join record
            var membership = new UserChatRoom
            {
                UserId = userId,
                ChatRoomId = chatRoomId,
                IsAdmin = true
            };

            // Assert - verify each property holds the correct value
            Assert.Equal(userId, membership.UserId);
            Assert.Equal(chatRoomId, membership.ChatRoomId);
            Assert.True(membership.IsAdmin);
        }

        // ============================================================
        // TEST 2: Verify default values
        // A new UserChatRoom should default IsAdmin to false
        // (regular member, not an admin).
        // ============================================================
        [Fact]
        public void UserChatRoom_DefaultConstructor_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act - create a blank membership
            var membership = new UserChatRoom();

            // Assert
            Assert.Equal(0, membership.UserId);         // FK defaults to 0
            Assert.Equal(0, membership.ChatRoomId);     // FK defaults to 0
            Assert.False(membership.IsAdmin);            // new members are NOT admins by default

            // JoinedAt should be set to "right now" automatically
            var timeDifference = DateTime.UtcNow - membership.JoinedAt;
            Assert.True(timeDifference.TotalSeconds < 1, "JoinedAt should be auto-set to current UTC time");
        }

        // ============================================================
        // TEST 3: Navigation properties should be null without EF
        // Same pattern as Message — EF fills these in at runtime.
        // ============================================================
        [Fact]
        public void UserChatRoom_NavigationProperties_ShouldBeNullWithoutEF()
        {
            // Arrange & Act
            var membership = new UserChatRoom();

            // Assert
            // Without Entity Framework, navigation properties are null.
            // This is expected behavior — EF populates them via Include() or lazy loading.
            Assert.Null(membership.User);
            Assert.Null(membership.ChatRoom);
        }

        // ============================================================
        // TEST 4: IsAdmin can be toggled
        // Verify we can promote a member to admin and demote them back.
        // ============================================================
        [Fact]
        public void UserChatRoom_IsAdmin_ShouldBeToggleable()
        {
            // Arrange - start as a regular member
            var membership = new UserChatRoom();
            Assert.False(membership.IsAdmin); // starts as false

            // Act - promote to admin
            membership.IsAdmin = true;

            // Assert - should now be admin
            Assert.True(membership.IsAdmin);

            // Act - demote back to regular member
            membership.IsAdmin = false;

            // Assert - should be regular member again
            Assert.False(membership.IsAdmin);
        }
    }
}
