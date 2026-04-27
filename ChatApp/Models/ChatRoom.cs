using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models;

public class ChatRoom
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedByUserId { get; set; }

    // Navigation properties
    public ICollection<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
