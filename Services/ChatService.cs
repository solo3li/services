using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Services;

public class ChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Message> SaveMessageAsync(string senderId, string receiverId,
        string content, int? orderId = null, string? attachmentUrl = null, string? attachmentName = null)
    {
        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            OrderId = orderId,
            AttachmentUrl = attachmentUrl,
            AttachmentName = attachmentName,
            ConversationKey = Message.BuildConversationKey(senderId, receiverId)
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();
        return msg;
    }

    public async Task<List<Message>> GetConversationAsync(string userId1, string userId2, int page = 1, int pageSize = 50)
    {
        var key = Message.BuildConversationKey(userId1, userId2);
        return await _db.Messages
            .Where(m => m.ConversationKey == key && m.OrderId == null)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Message>> GetOrderChatAsync(int orderId)
    {
        return await _db.Messages
            .Where(m => m.OrderId == orderId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<ConversationSummary>> GetConversationsAsync(string userId)
    {
        var messages = await _db.Messages
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && m.OrderId == null)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var groups = messages
            .GroupBy(m => m.ConversationKey)
            .Select(g =>
            {
                var latest = g.First();
                var otherId = latest.SenderId == userId ? latest.ReceiverId : latest.SenderId;
                var other = latest.SenderId == userId ? latest.Receiver : latest.Sender;
                return new ConversationSummary
                {
                    ConversationKey = g.Key,
                    OtherUserId = otherId,
                    OtherUserName = other?.FullName ?? "Unknown",
                    OtherUserAvatar = other?.Avatar,
                    LastMessage = latest.Content,
                    LastMessageAt = latest.SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return groups;
    }

    public async Task MarkReadAsync(string senderId, string receiverId)
    {
        var key = Message.BuildConversationKey(senderId, receiverId);
        var msgs = await _db.Messages
            .Where(m => m.ConversationKey == key && m.ReceiverId == receiverId && !m.IsRead)
            .ToListAsync();
        foreach (var m in msgs) m.IsRead = true;
        await _db.SaveChangesAsync();
    }
}

public class ConversationSummary
{
    public string ConversationKey { get; set; } = string.Empty;
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatar { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
