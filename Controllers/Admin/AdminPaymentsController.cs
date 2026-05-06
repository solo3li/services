using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminPaymentsController : Controller
{
    private readonly AppDbContext _db;

    public AdminPaymentsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // For now, we get all orders that have a payment (simulate transactions)
        var ordersWithPayments = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Service)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
            
        return View(ordersWithPayments);
    }
}
