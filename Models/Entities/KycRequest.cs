namespace ServicesApp.Models.Entities;

public class KycRequest
{
    public int Id { get; set; }
    public ExecutorStatus Status { get; set; } = ExecutorStatus.Pending;
    public string? IdCardImageUrl { get; set; }
    public string? SelfieUrl { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string? PaymentInfo { get; set; }
    public string? RejectionNote { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
