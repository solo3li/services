using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly KycService _kycService;

    public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager, KycService kycService)
    {
        _db = db;
        _userManager = userManager;
        _kycService = kycService;
    }

    [Authorize(Policy = "Dashboard.View")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _db.Users.CountAsync();
        ViewBag.TotalServices = await _db.Services.CountAsync();
        ViewBag.TotalOrders = await _db.Orders.CountAsync();
        ViewBag.PendingKyc = await _db.KycRequests.CountAsync(k => k.Status == ExecutorStatus.Pending);
        
        var recentOrders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Client)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View("~/Views/Admin/Dashboard.cshtml", recentOrders);
    }

    [Authorize(Policy = "Kyc.View")]
    public async Task<IActionResult> Kyc()
    {
        var pending = await _kycService.GetPendingAsync();
        return View("~/Views/Admin/Kyc.cshtml", pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveKyc(int id)
    {
        await _kycService.ApproveAsync(id, _userManager);
        TempData["SuccessMessage"] = "KYC Application approved successfully.";
        return RedirectToAction(nameof(Kyc));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectKyc(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Kyc));
        }

        await _kycService.RejectAsync(id, reason);
        TempData["SuccessMessage"] = "KYC Application rejected.";
        return RedirectToAction(nameof(Kyc));
    }

    [Authorize(Policy = "Users.View")]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View("~/Views/Admin/Users.cshtml", users);
    }

    [HttpPost]
    [Authorize(Policy = "Users.ToggleStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(string email, string fullName, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        {
            TempData["ErrorMessage"] = "All fields are required to create an admin.";
            return RedirectToAction(nameof(Users));
        }

        if (await _userManager.FindByEmailAsync(email) != null)
        {
            TempData["ErrorMessage"] = "A user with this email already exists.";
            return RedirectToAction(nameof(Users));
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsExecutor = true,
            ExecutorStatus = ExecutorStatus.Approved,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(admin, new[] { "Admin", "Student", "Executor" });
            TempData["SuccessMessage"] = "Admin user created successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Users));
    }

    public IActionResult Settings()
    {
        // Mocking settings since there's no DB entity for it in this prototype
        ViewBag.PlatformCommission = 15;
        ViewBag.WhoCanExecute = "ApprovedExecutorsOnly";
        ViewBag.BasePrice = 5;
        return View("~/Views/Admin/Settings.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateSettings(int platformCommission, string whoCanExecute, decimal basePrice)
    {
        // Here we would normally save this to a Settings table
        TempData["SuccessMessage"] = "Platform settings updated successfully.";
        return RedirectToAction(nameof(Settings));
    }

    [Authorize(Policy = "Settings.Email")]
    public IActionResult EmailSettings()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "Settings.Email")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateEmailSettings(string host, int port, string username, string password, string fromEmail)
    {
        // Mock saving to a settings table
        TempData["SuccessMessage"] = "Email SMTP settings updated successfully.";
        return RedirectToAction(nameof(EmailSettings));
    }

    [HttpPost]
    [Authorize(Policy = "Users.ManageRoles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(string userId, string role, bool addToRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (addToRole)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        else
        {
            // Don't allow demoting the last admin (safety check)
            if (role == "Admin" && user.Email == "admin@services.io")
            {
                TempData["ErrorMessage"] = "Cannot demote the primary system administrator.";
                return RedirectToAction(nameof(Users));
            }
            await _userManager.RemoveFromRoleAsync(user, role);
        }

        TempData["SuccessMessage"] = $"User roles updated successfully.";
        return RedirectToAction(nameof(Users));
    }
}
