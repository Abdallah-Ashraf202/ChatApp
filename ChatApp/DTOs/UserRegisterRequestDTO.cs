using ChatApp.Models;

namespace ChatApp.DTOs;

public class UserRegisterRequestDTO
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public Gender Gender { get; set; } = Gender.Other;
}
