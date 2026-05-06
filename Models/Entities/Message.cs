namespace ServicesApp.Models.Entities;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Conversation key: sorted "userId1_userId2"
    public string ConversationKey { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;

    public string ReceiverId { get; set; } = string.Empty;
    public ApplicationUser Receiver { get; set; } = null!;

    // If part of an order chat
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public static string BuildConversationKey(string userId1, string userId2)
    {
        var sorted = new[] { userId1, userId2 }.OrderBy(x => x).ToArray();
        return $"{sorted[0]}_{sorted[1]}";
    }
}
