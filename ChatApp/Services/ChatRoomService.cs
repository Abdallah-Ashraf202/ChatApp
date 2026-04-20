using Microsoft.EntityFrameworkCore;
using ChatApp.Models;
using ChatApp.DTOs;

namespace ChatApp.Services;

public class ChatRoomService
{
    private readonly ChatAppDbContext _context;

    public ChatRoomService(ChatAppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetChatRoomByIdAsync(int id)
    {
        return await _context.ChatRooms.FindAsync(id);
    }

    public async Task<IEnumerable<ChatRoomResponseDTO>> GetAllChatRoomsAsync()
    {
        return await _context.ChatRooms
            .Select(cr => new ChatRoomResponseDTO
            {
                Id = cr.Id,
                Name = cr.Name,
                Description = cr.Description,
                CreatedAt = cr.CreatedAt,
                CreatedByUserId = cr.CreatedByUserId,
                MemberCount = cr.UserChatRooms.Count
            })
            .ToListAsync();
    }

    // Returns only the rooms that a specific user has joined.
    public async Task<IEnumerable<ChatRoomResponseDTO>> GetUserRoomsAsync(int userId)
    {
        return await _context.UserChatRooms
            .Where(ucr => ucr.UserId == userId)
            .Select(ucr => new ChatRoomResponseDTO
            {
                Id = ucr.ChatRoom.Id,
                Name = ucr.ChatRoom.Name,
                Description = ucr.ChatRoom.Description,
                CreatedAt = ucr.ChatRoom.CreatedAt,
                CreatedByUserId = ucr.ChatRoom.CreatedByUserId,
                MemberCount = ucr.ChatRoom.UserChatRooms.Count
            })
            .ToListAsync();
    }

    // Creates a room and automatically joins the creator as an admin member.
    public async Task<ChatRoomResponseDTO> CreateChatRoomAsync(CreateChatRoomDTO dto, int creatorId)
    {
        var chatRoom = new ChatRoom
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedByUserId = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();

        // Adding Creator as Admin
        _context.UserChatRooms.Add(new UserChatRoom
        {
            UserId = creatorId,
            ChatRoomId = chatRoom.Id,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ChatRoomResponseDTO
        {
            Id = chatRoom.Id,
            Name = chatRoom.Name,
            Description = chatRoom.Description,
            CreatedAt = chatRoom.CreatedAt,
            CreatedByUserId = chatRoom.CreatedByUserId,
            MemberCount = 1
        };
    }

    public async Task<ChatRoom> UpdateChatRoomAsync(ChatRoom chatRoom)
    {
        _context.Entry(chatRoom).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<bool> DeleteChatRoomAsync(int id)
    {
        var chatRoom = await _context.ChatRooms.FindAsync(id);
        if (chatRoom == null) return false;

        _context.ChatRooms.Remove(chatRoom);
        await _context.SaveChangesAsync();
        return true;
    }

    // Joins a user to a chat room. No-ops if already a member.
    public async Task JoinRoomAsync(int userId, int chatRoomId)
    {
        var roomExists = await _context.ChatRooms.AnyAsync(cr => cr.Id == chatRoomId);
        if (!roomExists) throw new KeyNotFoundException("Chat room not found.");

        var alreadyMember = await _context.UserChatRooms
            .AnyAsync(ucr => ucr.UserId == userId && ucr.ChatRoomId == chatRoomId);

        if (alreadyMember) return; // idempotent

        _context.UserChatRooms.Add(new UserChatRoom
        {
            UserId = userId,
            ChatRoomId = chatRoomId,
            IsAdmin = false,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    //Removes a user from a chat room.
    public async Task LeaveRoomAsync(int userId, int chatRoomId)
    {
        var membership = await _context.UserChatRooms
            .FirstOrDefaultAsync(ucr => ucr.UserId == userId && ucr.ChatRoomId == chatRoomId)
            ?? throw new KeyNotFoundException("You are not a member of this chat room.");

        _context.UserChatRooms.Remove(membership);
        await _context.SaveChangesAsync();
    }
}
