using ChatApp.Models;
using ChatApp.DTOs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // All endpoints require a valid JWT by default
public class ChatRoomController : ControllerBase
{
    private readonly ChatRoomService _chatRoomService;

    public ChatRoomController(ChatRoomService chatRoomService)
    {
        _chatRoomService = chatRoomService;
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    private int GetCallerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── GET: api/chatroom ──────────────────────────────────────────────────
    /// <summary>Returns all available chat rooms.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatRoomResponseDTO>>> GetAllChatRooms()
    {
        var chatRooms = await _chatRoomService.GetAllChatRoomsAsync();
        return Ok(chatRooms);
    }

    // ── GET: api/chatroom/my ───────────────────────────────────────────────
    /// <summary>Returns only the rooms the current user has joined.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ChatRoomResponseDTO>>> GetMyRooms()
    {
        var rooms = await _chatRoomService.GetUserRoomsAsync(GetCallerId());
        return Ok(rooms);
    }

    // ── GET: api/chatroom/{id} ─────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<ActionResult<ChatRoom>> GetById(int id)
    {
        var chatRoom = await _chatRoomService.GetChatRoomByIdAsync(id);
        if (chatRoom == null) return NotFound();
        return Ok(chatRoom);
    }

    // ── POST: api/chatroom ─────────────────────────────────────────────────
    /// <summary>Creates a new chat room. Creator is auto-joined as admin.</summary>
    [HttpPost]
    public async Task<ActionResult<ChatRoomResponseDTO>> CreateChatRoom([FromBody] CreateChatRoomDTO dto)
    {
        try
        {
            var created = await _chatRoomService.CreateChatRoomAsync(dto, GetCallerId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── POST: api/chatroom/{id}/join ───────────────────────────────────────
    /// <summary>Current user joins the specified chat room.</summary>
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinRoom(int id)
    {
        try
        {
            await _chatRoomService.JoinRoomAsync(GetCallerId(), id);
            return Ok(new { message = "Joined successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── DELETE: api/chatroom/{id}/leave ───────────────────────────────────
    /// <summary>Current user leaves the specified chat room.</summary>
    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> LeaveRoom(int id)
    {
        try
        {
            await _chatRoomService.LeaveRoomAsync(GetCallerId(), id);
            return Ok(new { message = "Left successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── PUT: api/chatroom ──────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult<ChatRoom>> UpdateChatRoomAsync([FromBody] ChatRoom chatRoom)
    {
        var updatedChatRoom = await _chatRoomService.UpdateChatRoomAsync(chatRoom);
        return Ok(updatedChatRoom);
    }

    // ── DELETE: api/chatroom/{id} ──────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _chatRoomService.DeleteChatRoomAsync(id);
        if (deleted) return Ok();
        return NotFound();
    }
}


