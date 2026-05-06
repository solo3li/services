namespace ServicesApp.Models.Entities;

public class Service
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DeliveryDays { get; set; } = 3;
    public string? Thumbnail { get; set; }
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderCount { get; set; } = 0;
    public double Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string ExecutorId { get; set; } = string.Empty;
    public ApplicationUser Executor { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
