using ChatApp.Hubs;
using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Core services ─────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ChatAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=ChatAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"));

// ── Application services ──────────────────────────────────────────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChatRoomService>();
builder.Services.AddScoped<MessageService>();

// ── JWT Authentication ────────────────────────────────────────────────────
// NOTE: appsettings.json uses the lowercase "jwt" key; reading it in a
//       case-insensitive way so either casing works.
var jwtSection = builder.Configuration.GetSection("jwt");
var jwtKey     = jwtSection["Key"]     ?? throw new InvalidOperationException("jwt:Key is missing from appsettings.");
var jwtIssuer  = jwtSection["Issuer"]  ?? "ChatApp";
var jwtAudience = jwtSection["Audience"] ?? "ChatAppUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Allow SignalR to receive the token from the query string
        // because browser WebSocket headers can't carry Authorization.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// ── CORS (useful when a separate front-end connects) ──────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()   // required for SignalR
              .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");  // ws://localhost:{port}/hubs/chat

app.Run();
