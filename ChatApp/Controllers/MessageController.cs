using ChatApp.DTOs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatApp.Hubs;
namespace ChatApp.Controllers;
using Microsoft.AspNetCore.SignalR;


[Route("api/[controller]")]
[ApiController]
[Authorize] // All message endpoints require a valid JWT
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessageController(MessageService messageService)
    {
        _messageService = messageService;
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    private int GetCallerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool CallerIsAdmin() =>
        User.IsInRole("Admin");

    // ── GET: api/message/{chatRoomId} ──────────────────────────────────────
    /// Returns messages for a room, newest-first, paginated.
    /// ?page=1&amp;pageSize=50
    [HttpGet("{chatRoomId}")]
    public async Task<ActionResult<IEnumerable<MessageResponseDTO>>> GetMessages(
        int chatRoomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetRoomMessagesAsync(chatRoomId, page, pageSize);
        return Ok(messages);
    }

    // ── POST: api/message ──────────────────────────────────────────────────
    /// <summary>Posts a message to a chat room the caller is a member of.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MessageResponseDTO>> SendMessage([FromForm] SendMessageDTO dto)
    {
        try
        {
            var message = await _messageService.SendMessageAsync(GetCallerId(), dto);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── DELETE: api/message/{id} ───────────────────────────────────────────
    /// <summary>Deletes a message. Only the sender or an Admin can do this.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        try
        {
            await _messageService.DeleteMessageAsync(id, GetCallerId(), CallerIsAdmin());
            return Ok(new { message = "Message deleted." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
