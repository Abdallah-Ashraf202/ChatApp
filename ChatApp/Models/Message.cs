using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models;

public class Message
{
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public int ChatRoomId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}
