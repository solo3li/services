using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Client)
            .Include(o => o.Executor)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Client)
            .Include(o => o.Executor)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null) return NotFound();
        
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, OrderStatus newStatus)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = newStatus;
            
            if (newStatus == OrderStatus.Completed)
                order.CompletedAt = DateTime.UtcNow;
                
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Order status changed to {newStatus}.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
