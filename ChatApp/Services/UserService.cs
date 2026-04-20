using Microsoft.EntityFrameworkCore;
using ChatApp.Models;
using ChatApp.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using BCryptLib = BCrypt.Net.BCrypt;

namespace ChatApp.Services;

public class UserService
{
    private readonly ChatAppDbContext _context;
    private readonly IConfiguration _config;

    public UserService(ChatAppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }


    public async Task<User> CreateUserAsync(User user)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }


    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }


        return false;
    }

    public async Task<User> RegisterUser(UserRegisterRequestDTO userRegisterDTO)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userRegisterDTO.Email);
        if (existingUser != null)
        {
            throw new Exception("Email already in use.");
        }
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDTO.Password);
        var user = new User
        {
            Username = userRegisterDTO.Username!,
            Email = userRegisterDTO.Email!,
            PasswordHash = passwordHash,
            Role = userRegisterDTO.Role,
            Gender = userRegisterDTO.Gender
        };

        // Create the unique verification token
        user.VerificationToken = Guid.NewGuid().ToString();


        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }


    public async Task SendVerificationEmail(string email, string token)
    {
        // var verificationLink = $"http://localhost:5262/api/user/verify?token={token}";
        await Task.CompletedTask; // keeps the method async-compatible
    }

    public async Task<User> GetUserByToken(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
        if (user == null) throw new KeyNotFoundException($"User with token {token} not found.");
        return user;
    }

    public async Task<bool> VerifyEmailAsync(User user)
    {
        try
        {
            user.IsEmailVerified = true;
            user.VerificationToken = null; // Clear the token so it can't be reused
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying email for user {user.Email}: {ex.Message}");
            return false;
        }
        return true;
    }
    public async Task<User> Login(UserLoginRequestDTO LoginRequestDTO)
    {
        // 1. Find the user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == LoginRequestDTO.Email);

        // 2. Check if user exists AND verify the hashed password
        if (user == null || !BCrypt.Net.BCrypt.Verify(LoginRequestDTO.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Wrong email or password!");
        }
        if (!user.IsEmailVerified)
        {
            throw new InvalidOperationException("Please verify your email before logging in.");
        }
        return user;
    }
    public async Task<string> CreateToken(UserLoginRequestDTO LoginRequestDTO)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == LoginRequestDTO.Email);
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role.ToString())
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}