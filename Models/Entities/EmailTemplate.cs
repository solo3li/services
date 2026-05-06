using System.ComponentModel.DataAnnotations;

namespace ServicesApp.Models.Entities;

public class EmailTemplate
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    
    [Required, MaxLength(100)]
    public string Slug { get; set; } = ""; // e.g. "welcome-email", "order-placed"
    
    [Required, MaxLength(200)]
    public string Subject { get; set; } = "";
    
    [Required]
    public string Body { get; set; } = ""; // HTML Content with placeholders like {{UserName}}
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
