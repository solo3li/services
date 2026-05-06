using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminServicesController : Controller
{
    private readonly AppDbContext _db;

    public AdminServicesController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Policy = "Services.View")]
    public async Task<IActionResult> Index()
    {
        var services = await _db.Services
            .Include(s => s.Category)
            .Include(s => s.Executor)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return View(services);
    }

    [HttpPost]
    [Authorize(Policy = "Services.ToggleStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service != null)
        {
            service.IsActive = !service.IsActive;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Service {(service.IsActive ? "activated" : "deactivated")} successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "Services.Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service != null)
        {
            _db.Services.Remove(service);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}
