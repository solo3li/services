using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers;

public class ServicesController : Controller
{
    private readonly AppDbContext _db;
    private readonly FileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ServicesController(AppDbContext db, FileService fileService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _fileService = fileService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("Search", "Home");
    }

    public async Task<IActionResult> Details(int id)
    {
        var service = await _db.Services
            .Include(s => s.Category)
            .Include(s => s.Executor)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null) return NotFound();

        return View(service);
    }

    [Authorize(Roles = "Executor")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        return View(new CreateServiceViewModel());
    }

    [Authorize(Roles = "Executor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.ExecutorStatus != ExecutorStatus.Approved) 
            return Forbid();

        if (ModelState.IsValid)
        {
            string? thumbnailUrl = null;
            if (model.ThumbnailFile != null)
            {
                if (!_fileService.IsValidImage(model.ThumbnailFile))
                {
                    ModelState.AddModelError("ThumbnailFile", "Invalid image.");
                    ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
                    return View(model);
                }
                thumbnailUrl = await _fileService.SaveFileAsync(model.ThumbnailFile, "services");
            }

            var service = new Service
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                DeliveryDays = model.DeliveryDays,
                CategoryId = model.CategoryId,
                Tags = model.Tags,
                ExecutorId = user.Id,
                Thumbnail = thumbnailUrl,
                IsActive = true
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = service.Id });
        }

        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        return View(model);
    }
}

public class CreateServiceViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DeliveryDays { get; set; } = 3;
    public int CategoryId { get; set; }
    public string? Tags { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
}
