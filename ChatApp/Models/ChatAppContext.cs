using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
namespace ChatApp.Models;

public class ChatAppDbContext : DbContext
{
    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserChatRoom> UserChatRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Many-to-Many: User ↔ ChatRoom via UserChatRoom ──────────────
        modelBuilder.Entity<UserChatRoom>(entity =>
        {
            // Composite primary key
            entity.HasKey(uc => new { uc.UserId, uc.ChatRoomId });
 
            entity.HasOne(uc => uc.User)
                  .WithMany(u => u.UserChatRooms)
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uc => uc.ChatRoom)
                  .WithMany(cr => cr.UserChatRooms)
                  .HasForeignKey(uc => uc.ChatRoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── One-to-Many: ChatRoom → Messages ────────────────────────────
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.ChatRoom)
                  .WithMany(cr => cr.Messages)
                  .HasForeignKey(m => m.ChatRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ── One-to-Many: User → Messages ────────────────────────────
            entity.HasOne(m => m.User)
                  .WithMany(u => u.Messages)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent accidental user deletion wiping messages
        });

        // ── Optional: unique constraint on Username/Email ────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ── Optional: unique constraint on ChatRoom name ─────────────────
        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasIndex(cr => cr.Name).IsUnique();
        });
    }
}