using ServicesApp.Models.Entities;

namespace ServicesApp.Models.ViewModels;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Service> FeaturedServices { get; set; } = new();
}
