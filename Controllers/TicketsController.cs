using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> List()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var tickets = await _db.SupportTickets
            .Where(t => t.UserId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [HttpGet]
    public IActionResult Open(int? orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(string subject, string category, int? orderId, string message)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("", "Subject and message are required.");
            return View();
        }

        var ticket = new SupportTicket
        {
            UserId = user.Id,
            Subject = subject,
            Category = category,
            RelatedOrderId = orderId,
            Status = TicketStatus.Open
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        var initialMsg = new TicketMessage
        {
            TicketId = ticket.Id,
            SenderId = user.Id,
            Content = message,
            IsFromAdmin = false
        };

        _db.TicketMessages.Add(initialMsg);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Ticket created successfully. Our support team will respond shortly.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var ticket = await _db.SupportTickets
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null || (ticket.UserId != user.Id && !User.IsInRole("Admin")))
            return NotFound();

        ViewBag.CurrentUserId = user.Id;
        return View(ticket);
    }
}
