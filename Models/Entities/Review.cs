using System.ComponentModel.DataAnnotations;

namespace ServicesApp.Models.Entities;

public class Review
{
    public int Id { get; set; }
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
