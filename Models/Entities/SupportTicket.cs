namespace ServicesApp.Models.Entities;

public enum TicketStatus { Open, InProgress, Closed }

public class SupportTicket
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string Category { get; set; } = "General";
    public int? RelatedOrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}

public class TicketMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsFromAdmin { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public int TicketId { get; set; }
    public SupportTicket Ticket { get; set; } = null!;

    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;
}
