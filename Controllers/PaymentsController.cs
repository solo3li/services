using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServicesApp.Models.Entities;
using ServicesApp.Services;
using ServicesApp.Data;

namespace ServicesApp.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null || order.ClientId != user.Id || order.Status != OrderStatus.Pending)
            return NotFound();

        // Check if payment already exists
        var existingPayment = _db.Payments.FirstOrDefault(p => p.OrderId == orderId);
        if (existingPayment != null && existingPayment.Status == PaymentStatus.Completed)
        {
            return RedirectToAction("Tracking", "Orders", new { id = orderId });
        }

        ViewBag.OrderId = orderId;
        ViewBag.Amount = order.TotalPrice;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessStubPayment(int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null || order.ClientId != user.Id)
            return NotFound();

        // Stub logic - simulate Paymob success
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = order.TotalPrice,
            Status = PaymentStatus.Completed,
            PaymobRef = "STUB-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            TransactionId = "TXN-" + Guid.NewGuid().ToString().Substring(0, 10).ToUpper(),
            PaidAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return RedirectToAction("Result", new { orderId = orderId, success = true, txId = payment.TransactionId });
    }

    public async Task<IActionResult> Result(int orderId, bool success, string txId)
    {
        ViewBag.OrderId = orderId;
        ViewBag.Success = success;
        ViewBag.TxId = txId;
        return View();
    }
}
