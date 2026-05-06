using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServicesApp.Services;

namespace ServicesApp.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatService _chatService;
    private readonly NotificationService _notifService;

    public ChatHub(ChatService chatService, NotificationService notifService)
    {
        _chatService = chatService;
        _notifService = notifService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task SendDirectMessage(string receiverId, string content, string? orderId = null)
    {
        var senderId = Context.UserIdentifier!;
        int? orderIdInt = orderId != null ? int.TryParse(orderId, out var oid) ? oid : null : null;

        var message = await _chatService.SaveMessageAsync(senderId, receiverId, content, orderIdInt);

        var payload = new
        {
            id = message.Id,
            senderId = message.SenderId,
            senderName = message.Sender?.FullName ?? "Unknown",
            senderAvatar = message.Sender?.Avatar,
            content = message.Content,
            sentAt = message.SentAt.ToString("o"),
            isOwn = false
        };

        // Send to receiver
        await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", payload);
        // Echo back to sender with isOwn=true
        await Clients.Caller.SendAsync("ReceiveMessage", new
        {
            id = message.Id,
            senderId = message.SenderId,
            senderName = message.Sender?.FullName ?? "Unknown",
            senderAvatar = message.Sender?.Avatar,
            content = message.Content,
            sentAt = message.SentAt.ToString("o"),
            isOwn = true
        });

        // Send notification to receiver
        await _notifService.CreateAsync(receiverId,
            "New Message",
            $"You have a new message from {message.Sender?.FullName}",
            Models.Entities.NotificationType.Chat,
            $"/chat/{senderId}");
    }

    public async Task SendOrderMessage(int orderId, string receiverId, string content)
    {
        var senderId = Context.UserIdentifier!;
        var message = await _chatService.SaveMessageAsync(senderId, receiverId, content, orderId);

        var payload = new
        {
            id = message.Id,
            senderId = message.SenderId,
            senderName = message.Sender?.FullName ?? "Unknown",
            senderAvatar = message.Sender?.Avatar,
            content = message.Content,
            sentAt = message.SentAt.ToString("o")
        };

        await Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderMessage", payload);
    }

    public async Task JoinOrderChat(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task SendTyping(string receiverId)
    {
        await Clients.Group($"user_{receiverId}").SendAsync("UserTyping", Context.UserIdentifier);
    }
}
