using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServicesApp.Data;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ChatService _chatService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public ChatController(ChatService chatService, UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _chatService = chatService;
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Conversations()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var convos = await _chatService.GetConversationsAsync(user.Id);
        return View(convos);
    }

    public async Task<IActionResult> Direct(string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        if (userId == currentUser.Id) return RedirectToAction(nameof(Conversations));

        var otherUser = await _userManager.FindByIdAsync(userId);
        if (otherUser == null) return NotFound();

        await _chatService.MarkReadAsync(userId, currentUser.Id);

        var messages = await _chatService.GetConversationAsync(currentUser.Id, userId);
        
        ViewBag.OtherUserId = otherUser.Id;
        ViewBag.OtherUserName = otherUser.FullName;
        ViewBag.OtherUserAvatar = otherUser.Avatar;
        ViewBag.CurrentUserId = currentUser.Id;

        return View(messages);
    }

    public async Task<IActionResult> OrderChat(int orderId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        string receiverId = currentUser.Id == order.ClientId ? order.ExecutorId : order.ClientId;
        
        var messages = await _chatService.GetOrderChatAsync(orderId);
        
        ViewBag.OrderId = orderId;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.ReceiverId = receiverId;

        return View(messages);
    }
}
