using Xunit;
using ChatApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Tests;

public class MessageTests
{
    // ============================================================
    // TEST 1: Verify that properties are correctly assigned
    // We create a Message with specific values and check they stick.
    // ============================================================
    [Fact]
    public void Message_Initialization_ShouldSetPropertiesCorrectly()
    {
        // Arrange - prepare data
        var content = "Hello, World!";
        var userId = 5;
        var chatRoomId = 10;

        // Act - create the message
        var message = new Message
        {
            Content = content,
            UserId = userId,
            ChatRoomId = chatRoomId
        };

        // Assert - verify each property holds the correct value
        Assert.Equal(content, message.Content);
        Assert.Equal(userId, message.UserId);
        Assert.Equal(chatRoomId, message.ChatRoomId);
    }

    // ============================================================
    // TEST 2: Verify default values when no data is provided
    // A brand-new Message should have sensible defaults.
    // ============================================================
    [Fact]
    public void Message_DefaultConstructor_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act - create a blank message
        var message = new Message();

        // Assert
        Assert.Equal(0, message.Id);               // int defaults to 0
        Assert.Empty(message.Content);              // defaults to string.Empty
        Assert.Equal(0, message.UserId);            // FK defaults to 0
        Assert.Equal(0, message.ChatRoomId);        // FK defaults to 0

        // SentAt should be set to "right now" automatically
        var timeDifference = DateTime.UtcNow - message.SentAt;
        Assert.True(timeDifference.TotalSeconds < 1, "SentAt should be auto-set to current UTC time");
    }

    // ============================================================
    // TEST 3: Navigation properties should not be null
    // The model uses 'null!' to tell the compiler "trust me, EF will fill this in",
    // but in a unit test (without EF), they will actually be null.
    // This test documents that expected behavior.
    // ============================================================
    [Fact]
    public void Message_NavigationProperties_ShouldBeNullWithoutEF()
    {
        // Arrange & Act
        var message = new Message();

        // Assert
        // Without Entity Framework loading related data,
        // navigation properties initialized with 'null!' will be null.
        // This is expected — EF populates them at runtime via lazy/eager loading.
        Assert.Null(message.User);
        Assert.Null(message.ChatRoom);
    }

    // ============================================================
    // TEST 4: Validation should fail when Content is empty
    // The [Required] attribute on Content should trigger an error.
    // ============================================================
    [Fact]
    public void Message_Validation_ShouldFailWhenContentIsEmpty()
    {
        // Arrange - create a message with empty content
        var message = new Message { Content = "" };

        var validationContext = new ValidationContext(message);
        var validationResults = new List<ValidationResult>();

        // Act - run .NET's built-in validation
        var isValid = Validator.TryValidateObject(message, validationContext, validationResults, true);

        // Assert - should fail because Content is [Required]
        Assert.False(isValid, "Validation should fail when Content is empty");
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Content"));
    }

    // ============================================================
    // TEST 5: Validation should fail when Content exceeds 2000 chars
    // The [MaxLength(2000)] attribute on Content should catch this.
    // ============================================================
    [Fact]
    public void Message_Validation_ShouldFailWhenContentExceedsMaxLength()
    {
        // Arrange - create content that is 2001 characters long
        var message = new Message { Content = new string('X', 2001) };

        var validationContext = new ValidationContext(message);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(message, validationContext, validationResults, true);

        // Assert - should fail because 2001 > MaxLength of 2000
        Assert.False(isValid, "Validation should fail when Content exceeds 2000 characters");
    }

    // ============================================================
    // TEST 6: Validation should PASS with valid content
    // A message within the 2000-char limit should be accepted.
    // ============================================================
    [Fact]
    public void Message_Validation_ShouldPassWithValidContent()
    {
        // Arrange - a normal, valid message
        var message = new Message { Content = "This is a valid message." };

        var validationContext = new ValidationContext(message);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(message, validationContext, validationResults, true);

        // Assert - should pass with no validation errors
        Assert.True(isValid, "Validation should pass for valid content");
        Assert.Empty(validationResults);
    }
}

