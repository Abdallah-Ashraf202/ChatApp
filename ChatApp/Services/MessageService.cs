using ChatApp.DTOs;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public class MessageService
{
    private readonly ChatAppDbContext _context;

    public MessageService(ChatAppDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all messages in a room, newest first, with sender username.</summary>
    public async Task<IEnumerable<MessageResponseDTO>> GetRoomMessagesAsync(int chatRoomId, int page = 1, int pageSize = 50)
    {
        return await _context.Messages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MessageResponseDTO
            {
                Id = m.Id,
                ChatRoomId = m.ChatRoomId,
                UserId = m.UserId,
                Username = m.User.Username,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync();
    }

    // Posts a message. Validates that the sender is a member of the room.</summary>
    public async Task<MessageResponseDTO> SendMessageAsync(int senderId, SendMessageDTO dto)
    {
        // Ensure the user is a member of the chat room
        var isMember = await _context.UserChatRooms
            .AnyAsync(ucr => ucr.UserId == senderId && ucr.ChatRoomId == dto.ChatRoomId);

        if (!isMember)
            throw new UnauthorizedAccessException("You are not a member of this chat room.");

        var message = new Message
        {
            UserId = senderId,
            ChatRoomId = dto.ChatRoomId,
            Content = dto.Content,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Load sender username for the response
        var username = await _context.Users
            .Where(u => u.Id == senderId)
            .Select(u => u.Username)
            .FirstAsync();

        return new MessageResponseDTO
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            UserId = message.UserId,
            Username = username,
            Content = message.Content,
            SentAt = message.SentAt
        };
    }

    // Deletes a message. Only the original sender or an Admin may delete.</summary>
    public async Task DeleteMessageAsync(int messageId, int requesterId, bool isAdmin)
    {
        var message = await _context.Messages.FindAsync(messageId)
            ?? throw new KeyNotFoundException("Message not found.");

        if (!isAdmin && message.UserId != requesterId)
            throw new UnauthorizedAccessException("You cannot delete another user's message.");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
    }
}
