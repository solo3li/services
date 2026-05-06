using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Hubs;

[Authorize]
public class TicketHub : Hub
{
    private readonly AppDbContext _db;

    public TicketHub(AppDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task JoinTicket(int ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket_{ticketId}");
    }

    public async Task SendTicketMessage(int ticketId, string content)
    {
        var senderId = Context.UserIdentifier!;

        var ticket = await _db.SupportTickets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null || ticket.Status == TicketStatus.Closed) return;

        var sender = await _db.Users.FindAsync(senderId);
        bool isAdmin = Context.User?.IsInRole("Admin") ?? false;

        var msg = new TicketMessage
        {
            TicketId = ticketId,
            Content = content,
            SenderId = senderId,
            IsFromAdmin = isAdmin
        };
        _db.TicketMessages.Add(msg);

        if (ticket.Status == TicketStatus.Open && isAdmin)
            ticket.Status = TicketStatus.InProgress;

        await _db.SaveChangesAsync();

        await Clients.Group($"ticket_{ticketId}").SendAsync("ReceiveTicketMessage", new
        {
            id = msg.Id,
            content = msg.Content,
            isFromAdmin = msg.IsFromAdmin,
            senderName = sender?.FullName ?? "Unknown",
            sentAt = msg.SentAt.ToString("o")
        });
    }
}
