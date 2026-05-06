using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Controllers;

[Authorize]
public class ExecutorsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExecutorsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsExecutor) return RedirectToAction("Index", "Home");

        var orders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Client)
            .Where(o => o.ExecutorId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.TotalEarnings = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalPrice * 0.85m);
        ViewBag.PendingEarnings = orders.Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalPrice * 0.85m);
        ViewBag.ActiveOrdersCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Delivered);
        
        return View(orders);
    }

    public async Task<IActionResult> MyServices()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsExecutor) return RedirectToAction("Index", "Home");

        var services = await _db.Services
            .Include(s => s.Category)
            .Where(s => s.ExecutorId == user.Id)
            .ToListAsync();

        return View(services);
    }
}
