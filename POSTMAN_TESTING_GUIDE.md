# ChatApp Postman Testing Guide

This guide provides step-by-step instructions for testing the ChatApp backend using Postman.

## 1. Environment Setup
- **Base URL**: `https://localhost:7095` (or `http://localhost:5214`)
- **Headers**: Most requests require `Content-Type: application/json`.
- **Authorization**: For protected routes, use `Bearer <YOUR_TOKEN>`.

---

## 2. User Flow (REST API)

### A. Register a New User
- **Method**: `POST`
- **URL**: `{{BaseURL}}/api/Users/register`
- **Body**:
```json
{
  "username": "John",
  "email": "john@test.com",
  "password": "Password123!",
  "role": 0,
  "gender": 0
}
```
**Note**: Copy the **Verification Token** from the end of the response text.

### B. Verify Email
- **Method**: `GET`
- **URL**: `{{BaseURL}}/api/Users/verify?token=<PASTE_TOKEN_HERE>`
*Must be done before login!*

### C. Login
- **Method**: `POST`
- **URL**: `{{BaseURL}}/api/Users/login`
- **Body**:
```json
{
  "email": "john@test.com",
  "password": "Password123!"
}
```
**Note**: Copy the `token` from the response.

### D. Join Room
- **Method**: `POST`
- **URL**: `{{BaseURL}}/api/ChatRoom/1/join`
- **Headers**: `Authorization: Bearer <TOKEN>`

---

## 3. Real-Time Flow (SignalR)

SignalR uses WebSockets. In Postman, open a **New -> WebSocket** request.

### A. Connection
- **URL**: `wss://localhost:7095/hubs/chat?access_token=<YOUR_TOKEN>`
*Ensure there are NO angle brackets `<>` around the token.*

### B. Step 1: Handshake (MANDATORY)
Immediately after connecting, send this exact JSON to agree on the protocol:
```json
{"protocol":"json","version":1} 
```
> [!IMPORTANT]
> Every SignalR message must end with the **Record Separator** (ASCII 30) character ``. 

### C. Step 2: Join Group
Subscribe to messages for a specific room (e.g., Room 1):
```json
{"type":1,"target":"JoinRoomGroup","arguments":[1]} 
```

### D. Step 3: Send Message
Broadcast a message to the room:
```json
{
  "type": 1,
  "target": "SendMessage",
  "arguments": [
    {
      "ChatRoomId": 1,
      "Content": "Hello everyone!"
    }
  ]
} 
```

### E. Receiving Messages (User 2)
To test receiving, open a second Postman WebSocket tab as a different user. When User 1 sends a message, User 2 will see a `ReceiveMessage` object appear:
```json
{
  "type": 1,
  "target": "ReceiveMessage",
  "arguments": [{ "username": "John", "content": "Hello everyone!", ... }]
}
```

---

## 4. Common Troubleshooting
| Error | Cause | Solution |
| :--- | :--- | :--- |
| **401 Unauthorized** | Missing or invalid token | Check `access_token` query param. Remove any `<` or `>` brackets. |
| **405 Method Not Allowed** | Wrong HTTP Verb | Ensure Join/Register/Login are `POST` requests. |
| **Handshake was canceled** | Messages sent out of order | Always send the protocol handshake `{"protocol":"json","version":1}` FIRST. |
| **No Reply from Server** | Missing Record Separator | Add the ASCII 30 symbol `` to the end of every message. |
