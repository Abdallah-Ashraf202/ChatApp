namespace ChatApp.Models;

public class UserChatRoom
{
    public int UserId { get; set; }
    public int ChatRoomId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsAdmin { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}