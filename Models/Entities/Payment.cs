namespace ServicesApp.Models.Entities;

public enum PaymentStatus { Pending, Completed, Failed, Refunded }

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? PaymobRef { get; set; }
    public string? TransactionId { get; set; }
    public string Method { get; set; } = "Card";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
