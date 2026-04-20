# 💬 ChatApp — Real-Time Chat Application

A real-time chat application built with **ASP.NET Core** and **SignalR**, featuring JWT authentication, room-based messaging, and role-based access control.

---

## ✨ Features

- **Real-Time Messaging** — Instant message delivery via SignalR WebSockets.
- **Chat Rooms** — Create, join, and leave chat rooms with multi-user support.
- **JWT Authentication** — Secure token-based auth for both REST API and WebSocket connections.
- **Email Verification** — Token-based email verification flow before login is allowed.
- **Role-Based Access Control** — Admin and User roles with protected endpoints.
- **Paginated Message History** — Retrieve past messages with pagination support.
- **BCrypt Password Hashing** — Secure password storage using BCrypt.
- **Room Membership Management** — Many-to-many relationship between users and rooms with admin designation.

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core (.NET 10) |
| **Real-Time** | SignalR (WebSockets) |
| **Database** | SQL Server (LocalDB) |
| **ORM** | Entity Framework Core 10 |
| **Authentication** | JWT Bearer Tokens |
| **Password Security** | BCrypt.Net |
| **API Documentation** | OpenAPI |

---

## 📁 Project Structure

```
ChatApp/
├── Controllers/
│   ├── UserController.cs         # Registration, login, email verification
│   ├── ChatRoomController.cs     # CRUD for chat rooms, join/leave
│   └── MessageController.cs      # Send, retrieve, delete messages (REST)
├── Hubs/
│   └── ChatHub.cs                # SignalR hub for real-time messaging
├── Services/
│   ├── UserService.cs            # User logic, JWT generation, BCrypt hashing
│   ├── ChatRoomService.cs        # Room CRUD, membership management
│   └── MessageService.cs         # Message persistence & validation
├── Models/
│   ├── User.cs                   # User entity with roles & gender
│   ├── ChatRoom.cs               # Chat room entity
│   ├── Message.cs                # Message entity
│   ├── UserChatRoom.cs           # Many-to-many join table (User ↔ Room)
│   └── ChatAppContext.cs         # EF Core DbContext & relationships
├── DTOs/
│   ├── UserRegisterRequestDTO.cs # Registration payload
│   ├── UserLoginRequestDTO.cs    # Login payload
│   ├── ChatRoomDTO.cs            # Create & response DTOs for rooms
│   └── MessageDTO.cs             # Send & response DTOs for messages
├── Program.cs                    # App startup, DI, JWT config, SignalR mapping
└── appsettings.json              # Connection string & JWT settings
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/ChatApp.git
   cd ChatApp
   ```

2. **Update the connection string** (if needed) in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ChatAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Apply database migrations**
   ```bash
   cd ChatApp
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. The API will be available at `https://localhost:{port}`

---

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/users/register` | Register a new user |
| `GET` | `/api/users/verify?token=` | Verify email address |
| `POST` | `/api/users/login` | Login & receive JWT |

### Chat Rooms (🔒 JWT Required)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/chatroom` | List all chat rooms |
| `GET` | `/api/chatroom/my` | List rooms the current user joined |
| `GET` | `/api/chatroom/{id}` | Get a specific room |
| `POST` | `/api/chatroom` | Create a new room |
| `POST` | `/api/chatroom/{id}/join` | Join a room |
| `DELETE` | `/api/chatroom/{id}/leave` | Leave a room |
| `PUT` | `/api/chatroom` | Update a room (Admin only) |
| `DELETE` | `/api/chatroom/{id}` | Delete a room (Admin only) |

### Messages (🔒 JWT Required)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/message/{chatRoomId}?page=1&pageSize=50` | Get paginated room messages |
| `POST` | `/api/message` | Send a message (REST) |
| `DELETE` | `/api/message/{id}` | Delete a message |

### SignalR Hub (🔒 JWT via Query String)
| Connection URL | `ws://localhost:{port}/hubs/chat?access_token=JWT` |
|---|---|

| Client Method | Description |
|---|---|
| `JoinRoomGroup(chatRoomId)` | Subscribe to a room's live messages |
| `LeaveRoomGroup(chatRoomId)` | Unsubscribe from a room |
| `SendMessage({ chatRoomId, content })` | Send a message (persisted + broadcast) |

| Server Event | Description |
|---|---|
| `ReceiveMessage` | Fired when a new message is sent in a joined room |
| `Error` | Fired when an unauthorized action is attempted |

---

## 🔐 Authentication Flow

```
Register → Verify Email → Login → Receive JWT → Use JWT for API & SignalR
```

- **REST API**: Pass the JWT in the `Authorization: Bearer <token>` header.
- **SignalR**: Append as query string `?access_token=<token>` (WebSockets can't send custom headers).

---

## 🏗 Architecture Overview

```
Client (Browser / Mobile)
    │
    ├── REST API (HTTP)  ──→  Controllers  ──→  Services  ──→  EF Core  ──→  SQL Server
    │
    └── SignalR (WebSocket)  ──→  ChatHub  ──→  Services  ──→  EF Core  ──→  SQL Server
                                     │
                                     └──→  Broadcast to Group (real-time)
```

---

## 📄 License

This project is for educational and portfolio purposes.
