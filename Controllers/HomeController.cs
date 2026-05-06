using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.ViewModels;

namespace ServicesApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Take(8)
            .ToListAsync();

        var featuredServices = await _db.Services
            .Include(s => s.Category)
            .Include(s => s.Executor)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Rating)
            .ThenByDescending(s => s.OrderCount)
            .Take(6)
            .ToListAsync();

        return View(new HomeViewModel
        {
            Categories = categories,
            FeaturedServices = featuredServices
        });
    }

    public async Task<IActionResult> Categories()
    {
        var categories = await _db.Categories
            .Include(c => c.Services)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return View(categories);
    }

    public async Task<IActionResult> Search(string q, int? categoryId, decimal? maxPrice, string sort = "popular")
    {
        var query = _db.Services
            .Include(s => s.Category)
            .Include(s => s.Executor)
            .Where(s => s.IsActive);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(s => s.Title.Contains(q) || s.Tags.Contains(q) || s.Category.Name.Contains(q));

        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        if (maxPrice.HasValue)
            query = query.Where(s => s.Price <= maxPrice.Value);

        query = sort switch
        {
            "price_asc" => query.OrderBy(s => s.Price),
            "price_desc" => query.OrderByDescending(s => s.Price),
            "newest" => query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.OrderCount).ThenByDescending(s => s.Rating)
        };

        var results = await query.ToListAsync();
        var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();

        ViewBag.Query = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.Sort = sort;
        ViewBag.Categories = categories;

        return View(results);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
