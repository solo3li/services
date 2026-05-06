using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Services;

public class NotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(string userId, string title, string body,
        NotificationType type = NotificationType.Info, string? actionUrl = null)
    {
        var notif = new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            ActionUrl = actionUrl
        };
        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(string userId)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var notifs = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in notifs) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<List<Notification>> GetAllAsync(string userId, int page = 1)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * 20)
            .Take(20)
            .ToListAsync();
    }
}
