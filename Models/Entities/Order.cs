namespace ServicesApp.Models.Entities;

public enum OrderStatus
{
    Pending,
    InProgress,
    Delivered,
    Completed,
    Cancelled,
    Disputed
}

public class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Requirements { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? DeliveryFileUrl { get; set; }
    public string? DeliveryNote { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;

    public string? ExecutorId { get; set; }
    public ApplicationUser? Executor { get; set; }

    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
