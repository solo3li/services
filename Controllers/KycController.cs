using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServicesApp.Models.Entities;
using ServicesApp.Services;

namespace ServicesApp.Controllers;

[Authorize]
public class KycController : Controller
{
    private readonly KycService _kycService;
    private readonly FileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public KycController(KycService kycService, FileService fileService, UserManager<ApplicationUser> userManager)
    {
        _kycService = kycService;
        _fileService = fileService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Status()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var kyc = await _kycService.GetByUserIdAsync(user.Id);
        
        ViewBag.ExecutorStatus = user.ExecutorStatus;
        return View(kyc);
    }

    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (user.ExecutorStatus == ExecutorStatus.Approved)
            return RedirectToAction("Status");

        var existing = await _kycService.GetByUserIdAsync(user.Id);
        
        var model = new KycSubmitViewModel();
        if (existing != null)
        {
            model.Bio = existing.Bio;
            model.Skills = existing.Skills;
            model.PaymentInfo = existing.PaymentInfo;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(KycSubmitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        string? idCardUrl = null;
        string? selfieUrl = null;

        if (model.IdCardFile != null)
        {
            if (!_fileService.IsValidImage(model.IdCardFile))
            {
                ModelState.AddModelError("IdCardFile", "Invalid image file. Max size 5MB.");
                return View(model);
            }
            idCardUrl = await _fileService.SaveFileAsync(model.IdCardFile, "kyc");
        }

        if (model.SelfieFile != null)
        {
            if (!_fileService.IsValidImage(model.SelfieFile))
            {
                ModelState.AddModelError("SelfieFile", "Invalid image file. Max size 5MB.");
                return View(model);
            }
            selfieUrl = await _fileService.SaveFileAsync(model.SelfieFile, "kyc");
        }

        var existing = await _kycService.GetByUserIdAsync(user.Id);
        
        // Use existing URLs if not updated
        if (idCardUrl == null && existing != null) idCardUrl = existing.IdCardImageUrl;
        if (selfieUrl == null && existing != null) selfieUrl = existing.SelfieUrl;

        await _kycService.SubmitAsync(user.Id, model.Bio, model.Skills, idCardUrl, selfieUrl, model.PaymentInfo);

        // Update user status
        user.ExecutorStatus = ExecutorStatus.Pending;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Your application has been submitted successfully and is pending review.";
        return RedirectToAction("Status");
    }
}

public class KycSubmitViewModel
{
    [Required]
    [StringLength(1000, MinimumLength = 50)]
    public string Bio { get; set; } = string.Empty;

    [Required]
    public string Skills { get; set; } = string.Empty;

    public IFormFile? IdCardFile { get; set; }
    
    public IFormFile? SelfieFile { get; set; }

    [Required]
    public string PaymentInfo { get; set; } = string.Empty;
}
