using ChatApp.Models;
using ChatApp.DTOs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{

    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    // GET: api/<UsersController>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var Users = await _userService.GetAllUsersAsync();
        return Ok(Users);
    }


    // GET api/<UsersController>/5
    [Authorize(Roles = "Admin")] // Only Admins can access this endpoint
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var User = await _userService.GetUserByIdAsync(id);
        if (User == null) return NotFound();
        return Ok(User);
    }

    // POST api/<UsersController>
    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] User user)
    {
        var createdUser = await _userService.CreateUserAsync(user);
        return Ok(createdUser);
    }

    // PUT api/<UsersController>/5
    [HttpPut]
    public async Task<ActionResult<User>> UpdateUserAsync([FromBody] User user)
    {
        var UpdatedUser = await _userService.UpdateUserAsync(user);
        return Ok(UpdatedUser);
    }

    // DELETE api/<UsersController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var UserExists = await _userService.DeleteUserAsync(id);
        if (UserExists)
        {
            return Ok();
        }

        return NotFound();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] UserRegisterRequestDTO user)
    {
        try
        {
            var createdUser = await _userService.RegisterUser(user);
            // Send the Email (Logic below)
            await _userService.SendVerificationEmail(createdUser.Email, createdUser.VerificationToken!);
            return Ok($"User registered successfully! Please check your email to verify your account.{createdUser.VerificationToken}");
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Registration failed.",
                error = ex.Message
            });
        }
    }


    [HttpGet("verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            var user = await _userService.GetUserByToken(token);
            bool emailVerified = await _userService.VerifyEmailAsync(user);
            if (!emailVerified)
                return BadRequest("Failed to verify email.");

            return Ok("Email verified successfully! You can now log in.");
        }
        catch (KeyNotFoundException)
        {
            return BadRequest("Invalid token.");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginRequestDTO loginRequest)
    {
        try
        {
            var user = await _userService.Login(loginRequest);
            var token = await _userService.CreateToken(loginRequest); // Token creation belongs here or in a separate TokenService
            return Ok(new { message = "Success", token, userId = user.Id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
