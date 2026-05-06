using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ServicesApp.Data;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    private readonly OrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FileService _fileService;

    public OrdersController(AppDbContext db, OrderService orderService, UserManager<ApplicationUser> userManager, FileService fileService)
    {
        _db = db;
        _orderService = orderService;
        _userManager = userManager;
        _fileService = fileService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var orders = await _orderService.GetClientOrdersAsync(user.Id);
        return View("MyOrders", orders);
    }

    [Authorize(Roles = "Executor")]
    public async Task<IActionResult> ExecutorOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var orders = await _orderService.GetExecutorOrdersAsync(user.Id);
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _orderService.GetOrderWithDetailsAsync(id);
        if (order == null) return NotFound();

        // Security check
        if (order.ClientId != user.Id && order.ExecutorId != user.Id && !User.IsInRole("Admin"))
            return Forbid();

        return View(order);
    }

    public async Task<IActionResult> Tracking(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _orderService.GetOrderWithDetailsAsync(id);
        if (order == null) return NotFound();

        if (order.ClientId != user.Id && order.ExecutorId != user.Id && !User.IsInRole("Admin"))
            return Forbid();

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int serviceId)
    {
        var service = await _db.Services
            .Include(s => s.Executor)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null) return NotFound();

        var model = new CreateOrderViewModel
        {
            ServiceId = serviceId,
            ServiceTitle = service.Title,
            Price = service.Price,
            DeliveryDays = service.DeliveryDays
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _orderService.CreateOrderAsync(model.ServiceId, user.Id, model.Requirements);
        
        // Redirect to payment stub
        return RedirectToAction("Checkout", "Payments", new { orderId = order.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? note, IFormFile? deliveryFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        // Basic authorization
        if (status == OrderStatus.Completed && order.ClientId != user.Id) return Forbid(); // Only client can complete
        if ((status == OrderStatus.InProgress || status == OrderStatus.Delivered) && order.ExecutorId != user.Id) return Forbid(); // Only executor can progress/deliver

        string? deliveryFileUrl = null;
        if (deliveryFile != null && status == OrderStatus.Delivered)
        {
            if (!_fileService.IsValidFile(deliveryFile))
            {
                TempData["ErrorMessage"] = "Invalid delivery file.";
                return RedirectToAction(nameof(Tracking), new { id });
            }
            deliveryFileUrl = await _fileService.SaveFileAsync(deliveryFile, "deliveries");
        }

        await _orderService.UpdateStatusAsync(id, status, user.Id, note, deliveryFileUrl);
        
        TempData["SuccessMessage"] = "Order status updated successfully.";
        return RedirectToAction(nameof(Tracking), new { id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int orderId, int rating, string comment)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _db.Orders
            .Include(o => o.Service)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ClientId == user.Id);

        if (order == null || order.Status != OrderStatus.Completed || order.Review != null)
        {
            return BadRequest("Invalid request.");
        }

        var review = new Review
        {
            OrderId = orderId,
            ServiceId = order.ServiceId,
            UserId = user.Id,
            Rating = rating,
            Comment = comment
        };

        _db.Reviews.Add(review);
        
        // Update service rating stats
        var service = order.Service;
        var totalReviews = await _db.Reviews.CountAsync(r => r.ServiceId == service.Id) + 1;
        var totalRating = await _db.Reviews.Where(r => r.ServiceId == service.Id).SumAsync(r => r.Rating) + rating;
        service.Rating = (double)totalRating / totalReviews;
        service.ReviewCount = totalReviews;

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Thank you for your review!";
        
        return RedirectToAction(nameof(Tracking), new { id = orderId });
    }
}

public class CreateOrderViewModel
{
    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DeliveryDays { get; set; }

    [Required]
    public string Requirements { get; set; } = string.Empty;
}
