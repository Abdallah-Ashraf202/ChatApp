namespace ChatApp.DTOs;

/// <summary>Used when creating a new chat room.</summary>
public class CreateChatRoomDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Returned to clients — never exposes raw EF navigation lists.</summary>
public class ChatRoomResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public int MemberCount { get; set; }
}
