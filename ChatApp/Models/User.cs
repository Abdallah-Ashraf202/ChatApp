using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models;

public enum UserRole
{
    User,
    Admin
}

public enum Gender
{
    Male,
    Female,
    Other
}

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;
    
    public Gender Gender { get; set; } = Gender.Other;
    
    public string? VerificationToken { get; set; }
    
    public bool IsEmailVerified { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
