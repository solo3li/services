using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifService;

    public OrderService(AppDbContext db, NotificationService notifService)
    {
        _db = db;
        _notifService = notifService;
    }

    public async Task<Order> CreateOrderAsync(int serviceId, string clientId, string requirements)
    {
        var service = await _db.Services
            .Include(s => s.Executor)
            .FirstOrDefaultAsync(s => s.Id == serviceId)
            ?? throw new InvalidOperationException("Service not found");

        var order = new Order
        {
            ServiceId = serviceId,
            ClientId = clientId,
            ExecutorId = service.ExecutorId,
            Requirements = requirements,
            TotalPrice = service.Price,
            Status = OrderStatus.Pending
        };
        _db.Orders.Add(order);
        service.OrderCount++;
        await _db.SaveChangesAsync();

        await _notifService.CreateAsync(service.ExecutorId,
            "New Order Received! 🎉",
            $"You have a new order for '{service.Title}'",
            NotificationType.Order,
            $"/orders/{order.Id}");

        return order;
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _db.Orders
            .Include(o => o.Service).ThenInclude(s => s.Category)
            .Include(o => o.Client)
            .Include(o => o.Executor)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetClientOrdersAsync(string clientId)
    {
        return await _db.Orders
            .Include(o => o.Service).ThenInclude(s => s.Category)
            .Include(o => o.Executor)
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetExecutorOrdersAsync(string executorId)
    {
        return await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Client)
            .Where(o => o.ExecutorId == executorId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, string actorId,
        string? deliveryNote = null, string? deliveryFileUrl = null, string? cancellationReason = null)
    {
        var order = await _db.Orders
            .Include(o => o.Service)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = status;
        if (status == OrderStatus.Delivered)
        {
            order.DeliveredAt = DateTime.UtcNow;
            order.DeliveryNote = deliveryNote;
            order.DeliveryFileUrl = deliveryFileUrl;
        }
        if (status == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
            
            // Financial logic: Transfer funds to executor wallet
            // Deduct 15% platform commission
            var commission = order.TotalPrice * 0.15m;
            var earnings = order.TotalPrice - commission;

            var executor = await _db.Users.FindAsync(order.ExecutorId);
            if (executor != null)
            {
                executor.WalletBalance += earnings;
            }
        }

        if (cancellationReason != null) order.CancellationReason = cancellationReason;

        await _db.SaveChangesAsync();

        // Notify other party
        var notifyId = actorId == order.ClientId ? order.ExecutorId : order.ClientId;
        if (notifyId != null)
        {
            await _notifService.CreateAsync(notifyId,
                $"Order Status Updated",
                $"Order #{orderId} is now {status}",
                NotificationType.Order,
                $"/orders/{orderId}");
        }

        return true;
    }
}
