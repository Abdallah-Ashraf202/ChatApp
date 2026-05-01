using ChatApp.DTOs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChatApp.Hubs;


// SignalR hub that handles real-time messaging.
//
// Client connects → joins room group → sends/receives messages live.
// Every message posted via the REST API is also broadcast here,
// so REST and SignalR clients stay in sync.

[Authorize]
public class ChatHub : Hub
{
    private readonly MessageService _messageService;
    private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

    public ChatHub(MessageService messageService)
    {
        _messageService = messageService;
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private int GetCallerId() =>
        int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static string RoomGroup(int roomId) => $"room_{roomId}";

    // ── Connection lifecycle ───────────────────────────────────────────────
    public override async Task OnConnectedAsync()
    {
        var username = Context.User!.FindFirstValue(ClaimTypes.Name) ?? "Someone";
        _onlineUsers.TryAdd(Context.ConnectionId, username);

        // Notify everyone that a user is online
        await Clients.All.SendAsync("UserStatus", new { Username = username, Status = "Online" });
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_onlineUsers.TryRemove(Context.ConnectionId, out var username))
        {
            // Notify everyone that a user is offline
            await Clients.All.SendAsync("UserStatus", new { Username = username, Status = "Offline" });
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client can call this to get a list of all currently online usernames
    public Task<IEnumerable<string>> GetOnlineUsers()
    {
        return Task.FromResult(_onlineUsers.Values.Distinct());
    }

    // ── Client-callable methods ───────────────────────────────────────────

    // Client calls this when it opens a chat room.
    // Adds the connection to the SignalR group for that room.
    public async Task JoinRoomGroup(int chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(chatRoomId));
    }

    // Client calls this when it closes a chat room tab.
    public async Task LeaveRoomGroup(int chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(chatRoomId));
    }

    // ── Typing Indicators ─────────────────────────────────────────────────
    // Client calls this when the user starts or stops typing
    public async Task NotifyTyping(int chatRoomId, bool isTyping)
    {
        var username = Context.User!.FindFirstValue(ClaimTypes.Name) ?? "Someone";
        
        // Broadcast to everyone in the room EXCEPT the sender
        await Clients.GroupExcept(RoomGroup(chatRoomId), Context.ConnectionId)
            .SendAsync("UserTyping", new { Username = username, IsTyping = isTyping });
    }

    // Client sends a message. It is persisted via MessageService,
    // then broadcast to every connection in the room group.
    public async Task SendMessage(SendMessageDTO dto)
    {
        try
        {
            var message = await _messageService.SendMessageAsync(GetCallerId(), dto);

            // // Broadcast to all clients in the room (including sender)
            // await Clients
            //     .Group(RoomGroup(dto.ChatRoomId))
            //     .SendAsync("ReceiveMessage", message);
            // The MessageService will handle saving and broadcasting
            await _messageService.SendMessageAsync(GetCallerId(), dto);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Notify only the sender about the error
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }
}
