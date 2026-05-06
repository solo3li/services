using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminTicketsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminTicketsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var tickets = await _db.SupportTickets
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        return View(tickets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (ticket == null) return NotFound();
        
        ViewBag.Messages = ticket.Messages.OrderBy(m => m.SentAt).ToList();
        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser!.Id;
        
        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var ticket = await _db.SupportTickets.FindAsync(id);
        if (ticket != null)
        {
            ticket.Status = TicketStatus.Closed;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ticket closed successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}
