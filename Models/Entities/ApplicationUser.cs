using Microsoft.AspNetCore.Identity;

namespace ServicesApp.Models.Entities;

public enum ExecutorStatus { None, Pending, Approved, Rejected, Suspended }

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Bio { get; set; }
    public bool IsExecutor { get; set; } = false;
    public ExecutorStatus ExecutorStatus { get; set; } = ExecutorStatus.None;
    public decimal WalletBalance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Order> ClientOrders { get; set; } = new List<Order>();
    public ICollection<Order> ExecutorOrders { get; set; } = new List<Order>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public KycRequest? KycRequest { get; set; }
}
