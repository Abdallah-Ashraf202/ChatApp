namespace ChatApp.DTOs;

/// <summary>Payload a client sends to post a message.</summary>
public class SendMessageDTO
{
    public int ChatRoomId { get; set; }
    public string Content { get; set; } = string.Empty;
    public IFormFile? Media { get; set; } = null;
}

/// <summary>Returned to clients for a single message.</summary>
public class MessageResponseDTO
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Content { get; set; }

    public string? MediaUrl { get; set; }

    public DateTime SentAt { get; set; }
}
