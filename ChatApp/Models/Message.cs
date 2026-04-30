using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models;

public class Message
{
    public int Id { get; set; }

    [MaxLength(2000)]
    public string? Content { get; set; } = string.Empty; // text

    public string? MediaUrl { get; set; } = string.Empty; // image path


    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public int ChatRoomId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}
