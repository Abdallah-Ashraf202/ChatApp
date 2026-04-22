using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using BCryptLib = BCrypt.Net.BCrypt;

namespace ChatApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();

        // Ensure database is created/migrated
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // DB already seeded
        }

        // 1. Create Users (with a known password: "Password123!")
        var passwordHash = BCryptLib.HashPassword("Password123!");

        var testUsers = new List<User>
        {
            new User
            {
                Username = "Alice",
                Email = "alice@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                Gender = Gender.Female,
                IsEmailVerified = true // Auto-verify for easy testing
            },
            new User
            {
                Username = "Bob",
                Email = "bob@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.User,
                Gender = Gender.Male,
                IsEmailVerified = true // Auto-verify for easy testing
            }
        };

        context.Users.AddRange(testUsers);
        await context.SaveChangesAsync();

        // 2. Create a Chat Room
        var testRoom = new ChatRoom
        {
            Name = "General",
            Description = "The main testing room",
            CreatedByUserId = testUsers[0].Id, // Alice created it
            CreatedAt = DateTime.UtcNow
        };

        context.ChatRooms.Add(testRoom);
        await context.SaveChangesAsync();

        // 3. Add Users to the Chat Room
        var memberships = new List<UserChatRoom>
        {
            new UserChatRoom
            {
                UserId = testUsers[0].Id, // Alice
                ChatRoomId = testRoom.Id,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            },
            new UserChatRoom
            {
                UserId = testUsers[1].Id, // Bob
                ChatRoomId = testRoom.Id,
                IsAdmin = false,
                JoinedAt = DateTime.UtcNow
            }
        };

        context.UserChatRooms.AddRange(memberships);
        await context.SaveChangesAsync();

        // 4. Send some dummy messages
        var messages = new List<Message>
        {
            new Message
            {
                UserId = testUsers[0].Id,
                ChatRoomId = testRoom.Id,
                Content = "Hello everyone! Welcome to the new app.",
                SentAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new Message
            {
                UserId = testUsers[1].Id,
                ChatRoomId = testRoom.Id,
                Content = "Thanks Alice! Glad to be here.",
                SentAt = DateTime.UtcNow.AddMinutes(-2)
            }
        };

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
    }
}
