using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;

namespace ServicesApp.Controllers.Admin;

[Authorize(Policy = "Chat.View")]
public class AdminChatController : Controller
{
    private readonly AppDbContext _db;

    public AdminChatController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Get unique conversations based on ConversationKey
        // In a real app, we'd have a Conversation entity, but here we group by Key
        var conversations = await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .GroupBy(m => m.ConversationKey)
            .Select(g => new ChatMonitorViewModel
            {
                ConversationKey = g.Key,
                User1 = g.First().Sender.FullName,
                User2 = g.First().Receiver.FullName,
                MessageCount = g.Count(),
                LastMessageAt = g.Max(m => m.SentAt)
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        return View(conversations);
    }

    public async Task<IActionResult> Details(string key)
    {
        var messages = await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ConversationKey == key)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        if (!messages.Any()) return NotFound();

        ViewBag.User1 = messages.First().Sender.FullName;
        ViewBag.User2 = messages.First().Receiver.FullName;
        ViewBag.Key = key;

        return View(messages);
    }
}

public class ChatMonitorViewModel
{
    public string ConversationKey { get; set; } = "";
    public string User1 { get; set; } = "";
    public string User2 { get; set; } = "";
    public int MessageCount { get; set; }
    public DateTime LastMessageAt { get; set; }
}
